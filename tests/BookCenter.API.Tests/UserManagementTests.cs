using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Auth;
using BookCenter.Application.DTOs.Readers;
using BookCenter.Application.DTOs.Users;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookCenter.API.Tests
{
    public class UserManagementTests : IAsyncLifetime
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
        private const string ReaderPassword = "reader123";

        private readonly ApiFactory _factory = new();
        private HttpClient _anonymous = null!;
        private HttpClient _admin = null!;

        public async Task InitializeAsync()
        {
            _anonymous = _factory.CreateClient();
            _admin = await _factory.CreateAdminClientAsync();
        }

        public async Task DisposeAsync()
        {
            _anonymous.Dispose();
            _admin.Dispose();
            await _factory.DisposeAsync();
        }

        // ---------- helpers

        private sealed record Session(HttpClient Client, CurrentUserDto User);

        private Session ClientFor(AuthResponseDto auth)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
            return new Session(client, auth.User);
        }

        private async Task<Session> RegisterReaderAsync(string email)
        {
            var response = await _anonymous.PostAsJsonAsync("/api/auth/register",
                new { name = "Leyla", surname = "Mammadova", email, phone = "+994 50 123 45 67", password = ReaderPassword });
            Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
            return ClientFor((await response.Content.ReadFromJsonAsync<AuthResponseDto>(Json))!);
        }

        private Task<HttpResponseMessage> LoginAsync(string email, string password) =>
            _anonymous.PostAsJsonAsync("/api/auth/login", new { email, password });

        private async Task<string> AdminIdAsync()
        {
            var me = await _admin.GetFromJsonAsync<CurrentUserDto>("/api/auth/me", Json);
            return me!.Id;
        }

        private static async Task<string?> DetailOf(HttpResponseMessage response) =>
            (await response.Content.ReadFromJsonAsync<ProblemDetails>(Json))?.Detail;

        private static async Task<IDictionary<string, string[]>> FieldErrorsOf(HttpResponseMessage response) =>
            (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json))!.Errors;

        // ---------- tests

        [Fact]
        public async Task Only_admins_can_manage_users()
        {
            var reader = await RegisterReaderAsync("reader@test.local");

            Assert.Equal(HttpStatusCode.Unauthorized, (await _anonymous.GetAsync("/api/users")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await reader.Client.GetAsync("/api/users")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await reader.Client.PostAsync($"/api/users/{reader.User.Id}/unlock", null)).StatusCode);
        }

        [Fact]
        public async Task Lists_users_with_roles_and_reader_cards_and_filters_them()
        {
            var reader = await RegisterReaderAsync("leyla@test.local");

            var all = await _admin.GetFromJsonAsync<PagedResult<GetUserDto>>("/api/users", Json);
            Assert.Equal(2, all!.TotalCount);
            var listed = Assert.Single(all.Items, u => u.Id == reader.User.Id);
            Assert.Equal([Roles.Reader], listed.Roles);
            Assert.Equal(reader.User.ReaderId, listed.ReaderId);
            Assert.Null(listed.LockedUntil);

            var admins = await _admin.GetFromJsonAsync<PagedResult<GetUserDto>>("/api/users?role=admin", Json);
            Assert.Equal(ApiFactory.AdminEmail, Assert.Single(admins!.Items).Email);

            var search = await _admin.GetFromJsonAsync<PagedResult<GetUserDto>>("/api/users?search=leyla", Json);
            Assert.Equal(reader.User.Id, Assert.Single(search!.Items).Id);

            Assert.Equal(HttpStatusCode.BadRequest, (await _admin.GetAsync("/api/users?role=librarian")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _admin.GetAsync("/api/users/no-such-id")).StatusCode);
        }

        [Fact]
        public async Task Promoting_a_reader_ends_their_old_session_and_grants_admin_access()
        {
            var reader = await RegisterReaderAsync("promote@test.local");

            var response = await _admin.PutAsJsonAsync($"/api/users/{reader.User.Id}/admin", new { isAdmin = true });
            var updated = await response.Content.ReadFromJsonAsync<GetUserDto>(Json);
            Assert.Equal([Roles.Admin, Roles.Reader], updated!.Roles);

            // The old token still says "Reader" — it must stop working.
            Assert.Equal(HttpStatusCode.Unauthorized, (await reader.Client.GetAsync("/api/auth/me")).StatusCode);

            var promoted = await _factory.CreateClientAsAsync("promote@test.local", ReaderPassword);
            Assert.Equal(HttpStatusCode.OK, (await promoted.GetAsync("/api/readers")).StatusCode);

            // ...and back again.
            await _admin.PutAsJsonAsync($"/api/users/{reader.User.Id}/admin", new { isAdmin = false });
            Assert.Equal(HttpStatusCode.Unauthorized, (await promoted.GetAsync("/api/readers")).StatusCode);
        }

        [Fact]
        public async Task Admins_cannot_lock_out_or_demote_themselves()
        {
            var adminId = await AdminIdAsync();

            var demote = await _admin.PutAsJsonAsync($"/api/users/{adminId}/admin", new { isAdmin = false });
            Assert.Equal(HttpStatusCode.Conflict, demote.StatusCode);

            Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsync($"/api/users/{adminId}/lock", null)).StatusCode);

            var reset = await _admin.PostAsJsonAsync($"/api/users/{adminId}/reset-password", new { newPassword = "another123" });
            Assert.Equal(HttpStatusCode.Conflict, reset.StatusCode);

            // Still an admin with a working session.
            Assert.Equal(HttpStatusCode.OK, (await _admin.GetAsync("/api/users")).StatusCode);
        }

        [Fact]
        public async Task Demoting_an_admin_without_a_reader_card_is_refused()
        {
            var reader = await RegisterReaderAsync("second-admin@test.local");
            await _admin.PutAsJsonAsync($"/api/users/{reader.User.Id}/admin", new { isAdmin = true });
            var secondAdmin = await _factory.CreateClientAsAsync("second-admin@test.local", ReaderPassword);

            // The seeded admin has no reader card; without the role they couldn't sign in at all.
            var response = await secondAdmin.PutAsJsonAsync($"/api/users/{await AdminIdAsync()}/admin", new { isAdmin = false });
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Locking_ends_sessions_and_blocks_sign_in_until_unlocked()
        {
            var reader = await RegisterReaderAsync("lock@test.local");

            var locked = await (await _admin.PostAsync($"/api/users/{reader.User.Id}/lock", null))
                .Content.ReadFromJsonAsync<GetUserDto>(Json);
            Assert.NotNull(locked!.LockedUntil);

            Assert.Equal(HttpStatusCode.Unauthorized, (await reader.Client.GetAsync("/api/borrow-records")).StatusCode);

            var login = await LoginAsync("lock@test.local", ReaderPassword);
            Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
            Assert.Contains("locked", await DetailOf(login));

            var unlocked = await (await _admin.PostAsync($"/api/users/{reader.User.Id}/unlock", null))
                .Content.ReadFromJsonAsync<GetUserDto>(Json);
            Assert.Null(unlocked!.LockedUntil);
            Assert.Equal(HttpStatusCode.OK, (await LoginAsync("lock@test.local", ReaderPassword)).StatusCode);
        }

        [Fact]
        public async Task Unlock_also_lifts_a_failed_login_lockout()
        {
            var reader = await RegisterReaderAsync("forgetful@test.local");
            for (var i = 0; i < 5; i++)
                await LoginAsync("forgetful@test.local", "wrong-password1");

            Assert.Equal(HttpStatusCode.Unauthorized, (await LoginAsync("forgetful@test.local", ReaderPassword)).StatusCode);

            await _admin.PostAsync($"/api/users/{reader.User.Id}/unlock", null);
            Assert.Equal(HttpStatusCode.OK, (await LoginAsync("forgetful@test.local", ReaderPassword)).StatusCode);
        }

        [Fact]
        public async Task Admin_password_reset_validates_strength_and_ends_sessions()
        {
            var reader = await RegisterReaderAsync("reset@test.local");

            var weak = await _admin.PostAsJsonAsync($"/api/users/{reader.User.Id}/reset-password", new { newPassword = "short" });
            Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
            Assert.True((await FieldErrorsOf(weak)).ContainsKey("newPassword"));
            // A rejected password must not have touched the account.
            Assert.Equal(HttpStatusCode.OK, (await reader.Client.GetAsync("/api/auth/me")).StatusCode);

            var reset = await _admin.PostAsJsonAsync($"/api/users/{reader.User.Id}/reset-password", new { newPassword = "brandnew123" });
            Assert.Equal(HttpStatusCode.NoContent, reset.StatusCode);

            Assert.Equal(HttpStatusCode.Unauthorized, (await reader.Client.GetAsync("/api/auth/me")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await LoginAsync("reset@test.local", ReaderPassword)).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await LoginAsync("reset@test.local", "brandnew123")).StatusCode);
        }

        [Fact]
        public async Task Users_can_change_their_own_password()
        {
            var reader = await RegisterReaderAsync("self@test.local");

            var wrong = await reader.Client.PostAsJsonAsync("/api/auth/change-password",
                new { currentPassword = "not-my-password1", newPassword = "changed123" });
            Assert.Equal(HttpStatusCode.BadRequest, wrong.StatusCode);
            Assert.True((await FieldErrorsOf(wrong)).ContainsKey("currentPassword"));

            var same = await reader.Client.PostAsJsonAsync("/api/auth/change-password",
                new { currentPassword = ReaderPassword, newPassword = ReaderPassword });
            Assert.Equal(HttpStatusCode.BadRequest, same.StatusCode);

            var changed = await reader.Client.PostAsJsonAsync("/api/auth/change-password",
                new { currentPassword = ReaderPassword, newPassword = "changed123" });
            Assert.Equal(HttpStatusCode.OK, changed.StatusCode);

            // The returned token works; the old one (and any on other devices) doesn't.
            var fresh = ClientFor((await changed.Content.ReadFromJsonAsync<AuthResponseDto>(Json))!);
            Assert.Equal(HttpStatusCode.OK, (await fresh.Client.GetAsync("/api/auth/me")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await reader.Client.GetAsync("/api/auth/me")).StatusCode);

            Assert.Equal(HttpStatusCode.OK, (await LoginAsync("self@test.local", "changed123")).StatusCode);
        }

        [Fact]
        public async Task Changing_a_readers_email_changes_their_login_email()
        {
            var reader = await RegisterReaderAsync("old@test.local");
            var readerId = reader.User.ReaderId!.Value;

            var update = await _admin.PutAsJsonAsync($"/api/readers/{readerId}",
                new { name = "Leyla", surname = "Aliyeva", email = "New@Test.local", phone = "+994 50 123 45 67" });
            Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

            Assert.Equal(HttpStatusCode.Unauthorized, (await reader.Client.GetAsync("/api/auth/me")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await LoginAsync("old@test.local", ReaderPassword)).StatusCode);

            var login = await LoginAsync("new@test.local", ReaderPassword);
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            var user = (await login.Content.ReadFromJsonAsync<AuthResponseDto>(Json))!.User;
            Assert.Equal("new@test.local", user.Email);
            Assert.Equal("Leyla Aliyeva", user.FullName);

            // Can't take over another account's email.
            var taken = await _admin.PutAsJsonAsync($"/api/readers/{readerId}",
                new { name = "Leyla", surname = "Aliyeva", email = ApiFactory.AdminEmail, phone = "+994 50 123 45 67" });
            Assert.Equal(HttpStatusCode.Conflict, taken.StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await LoginAsync("new@test.local", ReaderPassword)).StatusCode);
        }

        [Fact]
        public async Task Deleting_a_card_ends_sessions_and_a_new_card_reactivates_the_account()
        {
            var reader = await RegisterReaderAsync("gone@test.local");
            Assert.Equal(HttpStatusCode.NoContent, (await _admin.DeleteAsync($"/api/readers/{reader.User.ReaderId}")).StatusCode);

            Assert.Equal(HttpStatusCode.Unauthorized, (await reader.Client.GetAsync("/api/auth/me")).StatusCode);
            var users = await _admin.GetFromJsonAsync<PagedResult<GetUserDto>>("/api/users?search=gone", Json);
            Assert.Null(Assert.Single(users!.Items).ReaderId);

            var card = await _admin.PostAsJsonAsync("/api/readers",
                new { name = "Leyla", surname = "Mammadova", email = "gone@test.local", phone = "+994 50 123 45 67" });
            var newCard = await card.Content.ReadFromJsonAsync<GetReaderDto>(Json);

            var login = await LoginAsync("gone@test.local", ReaderPassword);
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            Assert.Equal(newCard!.Id, (await login.Content.ReadFromJsonAsync<AuthResponseDto>(Json))!.User.ReaderId);
        }
    }
}
