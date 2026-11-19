using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Auth;
using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.BorrowRecords;
using BookCenter.Application.DTOs.Categories;
using BookCenter.Application.DTOs.Readers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookCenter.API.Tests
{
    public class AuthTests : IAsyncLifetime
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

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

        private static object Registration(string email, string password = "reader123") =>
            new { name = "Leyla", surname = "Mammadova", email, phone = "+994 50 123 45 67", password };

        private async Task<(HttpClient client, AuthResponseDto auth)> RegisterReaderAsync(string email)
        {
            var response = await _anonymous.PostAsJsonAsync("/api/auth/register", Registration(email));
            Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
            var auth = (await response.Content.ReadFromJsonAsync<AuthResponseDto>(Json))!;

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
            return (client, auth);
        }

        private async Task<T> AdminPostAsync<T>(string url, object body)
        {
            var response = await _admin.PostAsJsonAsync(url, body);
            Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
            return (await response.Content.ReadFromJsonAsync<T>(Json))!;
        }

        private async Task<GetBookDto> CreateBookAsync()
        {
            var author = await AdminPostAsync<GetBookItemDto>("/api/authors", new { name = "Author" });
            var category = await AdminPostAsync<GetBookItemDto>("/api/categories", new { name = "Category" });
            var publisher = await AdminPostAsync<GetBookItemDto>("/api/publishers", new { name = "Publisher" });
            return await AdminPostAsync<GetBookDto>("/api/books", new
            {
                name = "Book", isbn = "9789952123456", publishYear = 2020, totalCopies = 5,
                authorId = author.Id, categoryId = category.Id, publisherId = publisher.Id
            });
        }

        private static object Loan(int bookId, int readerId) => new { bookId, readerId, dueDate = DateTime.UtcNow.AddDays(7) };

        // ---------- tests

        [Fact]
        public async Task Catalog_is_public_but_everything_else_needs_a_login()
        {
            Assert.Equal(HttpStatusCode.OK, (await _anonymous.GetAsync("/api/books")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await _anonymous.GetAsync("/api/categories")).StatusCode);

            var write = await _anonymous.PostAsJsonAsync("/api/categories", new { name = "Nope" });
            Assert.Equal(HttpStatusCode.Unauthorized, write.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await _anonymous.GetAsync("/api/readers")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await _anonymous.GetAsync("/api/borrow-records")).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await _anonymous.GetAsync("/api/auth/me")).StatusCode);

            using var forged = _factory.CreateClient();
            forged.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");
            Assert.Equal(HttpStatusCode.Unauthorized, (await forged.GetAsync("/api/auth/me")).StatusCode);
        }

        [Fact]
        public async Task Registered_reader_gets_reader_role_and_cannot_use_admin_endpoints()
        {
            var (reader, auth) = await RegisterReaderAsync("leyla@example.com");

            Assert.Equal([Roles.Reader], auth.User.Roles);
            Assert.NotNull(auth.User.ReaderId);

            var me = await reader.GetFromJsonAsync<CurrentUserDto>("/api/auth/me", Json);
            Assert.Equal("leyla@example.com", me!.Email);
            Assert.Equal(auth.User.ReaderId, me.ReaderId);

            Assert.Equal(HttpStatusCode.OK, (await reader.GetAsync("/api/books")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await reader.PostAsJsonAsync("/api/categories", new { name = "Nope" })).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await reader.GetAsync("/api/readers")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await reader.PostAsJsonAsync("/api/borrow-records", Loan(1, auth.User.ReaderId!.Value))).StatusCode);
        }

        [Fact]
        public async Task Reader_only_sees_own_loans()
        {
            var book = await CreateBookAsync();
            var (leyla, leylaAuth) = await RegisterReaderAsync("leyla@example.com");
            var rauf = await AdminPostAsync<GetReaderDto>("/api/readers",
                new { name = "Rauf", surname = "Huseynov", email = "rauf@example.com", phone = "+994 55 000 00 00" });

            var leylaLoan = await AdminPostAsync<GetBorrowRecordDto>("/api/borrow-records", Loan(book.Id, leylaAuth.User.ReaderId!.Value));
            var raufLoan = await AdminPostAsync<GetBorrowRecordDto>("/api/borrow-records", Loan(book.Id, rauf.Id));

            // Even asking for Rauf's loans explicitly returns only Leyla's.
            var mine = await leyla.GetFromJsonAsync<PagedResult<GetBorrowRecordDto>>($"/api/borrow-records?readerId={rauf.Id}", Json);
            Assert.Equal([leylaLoan.Id], mine!.Items.Select(l => l.Id));

            Assert.Equal(HttpStatusCode.OK, (await leyla.GetAsync($"/api/borrow-records/{leylaLoan.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await leyla.GetAsync($"/api/borrow-records/{raufLoan.Id}")).StatusCode);

            var all = await _admin.GetFromJsonAsync<PagedResult<GetBorrowRecordDto>>("/api/borrow-records", Json);
            Assert.Equal(2, all!.TotalCount);
        }

        [Fact]
        public async Task Registering_with_an_existing_card_email_links_the_card()
        {
            var card = await AdminPostAsync<GetReaderDto>("/api/readers",
                new { name = "Rauf", surname = "Huseynov", email = "rauf@example.com", phone = "+994 55 000 00 00" });

            var (_, auth) = await RegisterReaderAsync("Rauf@Example.com");

            Assert.Equal(card.Id, auth.User.ReaderId);
            var readers = await _admin.GetFromJsonAsync<PagedResult<GetReaderDto>>("/api/readers", Json);
            Assert.Equal(1, readers!.TotalCount);
            Assert.Equal("Rauf", readers.Items[0].Name); // staff-entered details are kept
        }

        [Fact]
        public async Task Registration_validates_password_and_rejects_duplicates()
        {
            var weak = await _anonymous.PostAsJsonAsync("/api/auth/register", Registration("weak@example.com", "short"));
            Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
            var problem = await weak.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
            Assert.True(problem!.Errors.ContainsKey("password"), string.Join(",", problem.Errors.Keys));

            await RegisterReaderAsync("dup@example.com");
            var duplicate = await _anonymous.PostAsJsonAsync("/api/auth/register", Registration("dup@example.com"));
            Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        }

        [Fact]
        public async Task Wrong_password_is_401_and_repeated_failures_lock_the_account()
        {
            await RegisterReaderAsync("leyla@example.com");
            var wrong = new { email = "leyla@example.com", password = "wrong-password1" };

            var first = await _anonymous.PostAsJsonAsync("/api/auth/login", wrong);
            Assert.Equal(HttpStatusCode.Unauthorized, first.StatusCode);
            Assert.Equal("Invalid email or password.", (await first.Content.ReadFromJsonAsync<ProblemDetails>(Json))!.Detail);

            for (var i = 0; i < 4; i++) await _anonymous.PostAsJsonAsync("/api/auth/login", wrong);

            // Now even the right password is refused until the lockout expires.
            var locked = await _anonymous.PostAsJsonAsync("/api/auth/login", new { email = "leyla@example.com", password = "reader123" });
            Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
            Assert.Contains("Too many failed attempts", (await locked.Content.ReadFromJsonAsync<ProblemDetails>(Json))!.Detail);
        }

        [Fact]
        public async Task Reader_whose_card_was_deleted_cannot_sign_in()
        {
            var (_, auth) = await RegisterReaderAsync("leyla@example.com");
            Assert.Equal(HttpStatusCode.NoContent, (await _admin.DeleteAsync($"/api/readers/{auth.User.ReaderId}")).StatusCode);

            var login = await _anonymous.PostAsJsonAsync("/api/auth/login", new { email = "leyla@example.com", password = "reader123" });
            Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        }

        [Fact]
        public async Task Audit_columns_record_who_made_the_change()
        {
            var category = await AdminPostAsync<GetCategoryDto>("/api/categories", new { name = "Audited" });
            await _admin.PutAsJsonAsync($"/api/categories/{category.Id}", new { name = "Audited 2" });
            await _admin.DeleteAsync($"/api/categories/{category.Id}");

            await _factory.SeedAsync(async db =>
            {
                var row = await db.Categories.IgnoreQueryFilters().SingleAsync(c => c.Id == category.Id);
                Assert.Equal(ApiFactory.AdminEmail, row.CreatedBy);
                Assert.Equal(ApiFactory.AdminEmail, row.ModifiedBy);
                Assert.Equal(ApiFactory.AdminEmail, row.DeletedBy);
            });
        }
    }
}
