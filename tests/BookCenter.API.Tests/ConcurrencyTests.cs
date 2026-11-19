using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.Readers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookCenter.API.Tests
{
    /// <summary>
    /// Simultaneous requests racing for the same stock. The rowversion on Book turns the losers into
    /// 409s; these are most meaningful with BOOKCENTER_TEST_SQLSERVER set (see ApiFactory).
    /// </summary>
    public class ConcurrencyTests : IAsyncLifetime
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        private readonly ApiFactory _factory = new();
        private HttpClient _admin = null!;

        public async Task InitializeAsync() => _admin = await _factory.CreateAdminClientAsync();

        public async Task DisposeAsync()
        {
            _admin.Dispose();
            await _factory.DisposeAsync();
        }

        private async Task<T> PostAsync<T>(string url, object body)
        {
            var response = await _admin.PostAsJsonAsync(url, body);
            Assert.True(response.StatusCode == HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
            return (await response.Content.ReadFromJsonAsync<T>(Json))!;
        }

        private async Task<GetBookDto> CreateBookAsync(int copies)
        {
            var author = await PostAsync<GetBookItemDto>("/api/authors", new { name = "Author" });
            var category = await PostAsync<GetBookItemDto>("/api/categories", new { name = "Category" });
            var publisher = await PostAsync<GetBookItemDto>("/api/publishers", new { name = "Publisher" });
            return await PostAsync<GetBookDto>("/api/books", new
            {
                name = "Book", isbn = "9789952123456", publishYear = 2020, totalCopies = copies,
                authorId = author.Id, categoryId = category.Id, publisherId = publisher.Id
            });
        }

        private async Task<int[]> CreateReadersAsync(int count)
        {
            var ids = new int[count];
            for (var i = 0; i < count; i++)
                ids[i] = (await PostAsync<GetReaderDto>("/api/readers",
                    new { name = "Reader", surname = $"No{(char)('a' + i)}", email = $"r{i}@test.local", phone = "+994501234567" })).Id;
            return ids;
        }

        [SqlServerFact]
        public async Task Only_one_of_several_simultaneous_borrows_gets_the_last_copy()
        {
            var book = await CreateBookAsync(copies: 1);
            var readers = await CreateReadersAsync(6);

            var responses = await Task.WhenAll(readers.Select(readerId =>
                _admin.PostAsJsonAsync("/api/borrow-records", new { bookId = book.Id, readerId, dueDate = DateTime.UtcNow.AddDays(7) })));

            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
            Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.Created),
                r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));

            var after = await _admin.GetFromJsonAsync<GetBookDto>($"/api/books/{book.Id}", Json);
            Assert.Equal(0, after!.CopiesAvailable);
        }

        [SqlServerFact]
        public async Task Returning_the_same_loan_twice_at_once_restocks_only_one_copy()
        {
            var book = await CreateBookAsync(copies: 2);
            var reader = (await CreateReadersAsync(1))[0];
            var loan = await PostAsync<GetBookItemDto>("/api/borrow-records", new { bookId = book.Id, readerId = reader, dueDate = DateTime.UtcNow.AddDays(7) });

            var responses = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ =>
                _admin.PostAsync($"/api/borrow-records/{loan.Id}/return", null)));

            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.NoContent);
            Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.NoContent),
                r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));

            var after = await _admin.GetFromJsonAsync<GetBookDto>($"/api/books/{book.Id}", Json);
            Assert.Equal(2, after!.CopiesAvailable);
        }
    }
}
