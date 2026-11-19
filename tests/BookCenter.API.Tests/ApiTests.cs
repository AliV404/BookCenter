using BookCenter.Application.Common;
using BookCenter.Application.DTOs.Authors;
using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.BorrowRecords;
using BookCenter.Application.DTOs.Categories;
using BookCenter.Application.DTOs.Readers;
using BookCenter.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookCenter.API.Tests
{
    /// <summary>Library behaviour, exercised as the seeded admin. Security rules live in AuthTests.</summary>
    public class ApiTests : IAsyncLifetime
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

        private readonly ApiFactory _factory = new();
        private HttpClient _client = null!;

        public async Task InitializeAsync() => _client = await _factory.CreateAdminClientAsync();

        public async Task DisposeAsync()
        {
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        // ---------- helpers ----------

        private async Task<T> PostAsync<T>(string url, object body)
        {
            var response = await _client.PostAsJsonAsync(url, body);
            Assert.True(response.StatusCode == HttpStatusCode.Created, $"POST {url} → {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
            return (await response.Content.ReadFromJsonAsync<T>(Json))!;
        }

        private async Task<T> GetAsync<T>(string url) => (await _client.GetFromJsonAsync<T>(url, Json))!;

        private async Task<(int authorId, int categoryId, int publisherId)> CreateReferencesAsync()
        {
            var author = await PostAsync<GetAuthorDto>("/api/authors", new { name = "Nizami Gəncəvi", biography = "Poet" });
            var category = await PostAsync<GetCategoryDto>("/api/categories", new { name = "Poetry" });
            var publisher = await PostAsync<GetBookItemDto>("/api/publishers", new { name = "Qanun" });
            return (author.Id, category.Id, publisher.Id);
        }

        private async Task<GetBookDto> CreateBookAsync(int totalCopies = 2, string isbn = "9789952123456")
        {
            var (authorId, categoryId, publisherId) = await CreateReferencesAsync();
            return await PostAsync<GetBookDto>("/api/books", new
            {
                name = "Khamsa", isbn, publishYear = 2015, totalCopies, authorId, categoryId, publisherId
            });
        }

        private Task<GetReaderDto> CreateReaderAsync(string email = "leyla@example.com") =>
            PostAsync<GetReaderDto>("/api/readers", new { name = "Leyla", surname = "Mammadova", email, phone = "+994 50 123 45 67" });

        private static object Loan(int bookId, int readerId) =>
            new { bookId, readerId, dueDate = DateTime.UtcNow.AddDays(14) };

        // ---------- tests ----------

        [Fact]
        public async Task Borrow_and_return_keeps_stock_in_sync()
        {
            var book = await CreateBookAsync(totalCopies: 2);
            var reader = await CreateReaderAsync();

            var loan = await PostAsync<GetBorrowRecordDto>("/api/borrow-records", Loan(book.Id, reader.Id));
            Assert.Equal("Leyla Mammadova", loan.ReaderFullName);
            Assert.Null(loan.ReturnedAt);
            Assert.Equal(1, (await GetAsync<GetBookDto>($"/api/books/{book.Id}")).CopiesAvailable);
            Assert.Equal(1, (await GetAsync<GetReaderDto>($"/api/readers/{reader.Id}")).ActiveLoans);

            // Same reader can't hold two copies of the same book.
            var again = await _client.PostAsJsonAsync("/api/borrow-records", Loan(book.Id, reader.Id));
            Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);

            var returned = await _client.PostAsync($"/api/borrow-records/{loan.Id}/return", null);
            Assert.Equal(HttpStatusCode.NoContent, returned.StatusCode);
            Assert.Equal(2, (await GetAsync<GetBookDto>($"/api/books/{book.Id}")).CopiesAvailable);

            var returnedTwice = await _client.PostAsync($"/api/borrow-records/{loan.Id}/return", null);
            Assert.Equal(HttpStatusCode.Conflict, returnedTwice.StatusCode);
        }

        [Fact]
        public async Task Borrow_fails_when_no_copies_left()
        {
            var book = await CreateBookAsync(totalCopies: 1);
            var first = await CreateReaderAsync("a@example.com");
            var second = await CreateReaderAsync("b@example.com");

            await PostAsync<GetBorrowRecordDto>("/api/borrow-records", Loan(book.Id, first.Id));
            var response = await _client.PostAsJsonAsync("/api/borrow-records", Loan(book.Id, second.Id));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json);
            Assert.Contains("no copies available", problem!.Detail);
        }

        [Fact]
        public async Task Borrow_with_unknown_reader_returns_404()
        {
            var book = await CreateBookAsync();
            var response = await _client.PostAsJsonAsync("/api/borrow-records", Loan(book.Id, readerId: 999));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Borrow_with_past_due_date_is_a_validation_error()
        {
            var book = await CreateBookAsync();
            var reader = await CreateReaderAsync();
            var response = await _client.PostAsJsonAsync("/api/borrow-records",
                new { bookId = book.Id, readerId = reader.Id, dueDate = DateTime.UtcNow.AddDays(-1) });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
            Assert.True(problem!.Errors.ContainsKey("dueDate"));
        }

        [Fact]
        public async Task Overdue_filter_returns_only_late_loans()
        {
            var book = await CreateBookAsync(totalCopies: 3);
            var reader = await CreateReaderAsync();
            await PostAsync<GetBorrowRecordDto>("/api/borrow-records", Loan(book.Id, reader.Id)); // on time

            // A late loan can't be created through the API (validator), so seed it directly.
            await _factory.SeedAsync(db =>
            {
                db.BorrowRecords.Add(new BorrowRecord
                {
                    BookId = book.Id, ReaderId = reader.Id,
                    BorrowedAt = DateTime.UtcNow.AddDays(-30), DueDate = DateTime.UtcNow.AddDays(-2)
                });
                return Task.CompletedTask;
            });

            var overdue = await GetAsync<PagedResult<GetBorrowRecordDto>>("/api/borrow-records?status=overdue");
            var active = await GetAsync<PagedResult<GetBorrowRecordDto>>("/api/borrow-records?status=active");

            Assert.Equal(1, overdue.TotalCount);
            Assert.True(overdue.Items[0].IsOverdue);
            Assert.Equal(2, active.TotalCount);
        }

        [Fact]
        public async Task Validation_errors_use_camelCase_keys_and_unicode_names_are_allowed()
        {
            var bad = await _client.PostAsJsonAsync("/api/authors", new { name = "", biography = "x" });
            Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
            var problem = await bad.Content.ReadFromJsonAsync<ValidationProblemDetails>(Json);
            Assert.True(problem!.Errors.ContainsKey("name"));

            var symbols = await _client.PostAsJsonAsync("/api/authors", new { name = "!!!@@a" });
            Assert.Equal(HttpStatusCode.BadRequest, symbols.StatusCode);

            var azerbaijani = await _client.PostAsJsonAsync("/api/authors", new { name = "Məmməd Səid Ordubadi" });
            Assert.Equal(HttpStatusCode.Created, azerbaijani.StatusCode);
        }

        [Fact]
        public async Task Unknown_ids_return_404_problem_details()
        {
            var response = await _client.GetAsync("/api/books/999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Json);
            Assert.Equal(404, problem!.Status);

            var update = await _client.PutAsJsonAsync("/api/categories/999", new { name = "Whatever" });
            Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        }

        [Fact]
        public async Task Category_with_books_cannot_be_deleted_and_deletes_are_soft()
        {
            var book = await CreateBookAsync();

            var blocked = await _client.DeleteAsync($"/api/categories/{book.CategoryId}");
            Assert.Equal(HttpStatusCode.Conflict, blocked.StatusCode);

            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/books/{book.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/categories/{book.CategoryId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/categories/{book.CategoryId}")).StatusCode);

            // Soft delete: the row is still in the database, just hidden by the query filter.
            await _factory.SeedAsync(async db =>
            {
                var deleted = await db.Books.IgnoreQueryFilters().SingleAsync(b => b.Id == book.Id);
                Assert.True(deleted.IsDeleted);
                Assert.NotNull(deleted.DeletedAt);
            });

            // ...and the name can be reused afterwards.
            var recreated = await _client.PostAsJsonAsync("/api/categories", new { name = "Poetry" });
            Assert.Equal(HttpStatusCode.Created, recreated.StatusCode);
        }

        [Fact]
        public async Task Book_with_active_loan_cannot_be_deleted_or_shrunk_below_loans()
        {
            var book = await CreateBookAsync(totalCopies: 2);
            var reader = await CreateReaderAsync();
            await PostAsync<GetBorrowRecordDto>("/api/borrow-records", Loan(book.Id, reader.Id));

            Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync($"/api/books/{book.Id}")).StatusCode);

            var update = new
            {
                name = book.Name, isbn = book.ISBN, publishYear = book.PublishYear,
                authorId = book.AuthorId, categoryId = book.CategoryId, publisherId = book.PublisherId,
                totalCopies = 0
            };
            Assert.Equal(HttpStatusCode.Conflict, (await _client.PutAsJsonAsync($"/api/books/{book.Id}", update)).StatusCode);

            var grow = await _client.PutAsJsonAsync($"/api/books/{book.Id}", update with { totalCopies = 5 });
            Assert.Equal(HttpStatusCode.NoContent, grow.StatusCode);
            var updated = await GetAsync<GetBookDto>($"/api/books/{book.Id}");
            Assert.Equal(5, updated.TotalCopies);
            Assert.Equal(4, updated.CopiesAvailable);
        }

        [Fact]
        public async Task Duplicate_isbn_and_missing_references_are_rejected()
        {
            var book = await CreateBookAsync();
            var duplicate = await _client.PostAsJsonAsync("/api/books", new
            {
                name = "Copy", isbn = book.ISBN, publishYear = 2000, totalCopies = 1,
                authorId = book.AuthorId, categoryId = book.CategoryId, publisherId = book.PublisherId
            });
            Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

            var missingAuthor = await _client.PostAsJsonAsync("/api/books", new
            {
                name = "Orphan", isbn = "9780000000001", publishYear = 2000, totalCopies = 1,
                authorId = 999, categoryId = book.CategoryId, publisherId = book.PublisherId
            });
            Assert.Equal(HttpStatusCode.NotFound, missingAuthor.StatusCode);
        }

        [Fact]
        public async Task Paging_is_clamped_and_reports_totals()
        {
            for (var i = 1; i <= 15; i++)
                await PostAsync<GetCategoryDto>("/api/categories", new { name = $"Category {i:00}" });

            var page2 = await GetAsync<PagedResult<GetCategoryItemDto>>("/api/categories?page=2&pageSize=10");
            Assert.Equal(15, page2.TotalCount);
            Assert.Equal(5, page2.Items.Count);
            Assert.Equal("Category 11", page2.Items[0].Name);

            var clamped = await GetAsync<PagedResult<GetCategoryItemDto>>("/api/categories?page=0&pageSize=100000");
            Assert.Equal(1, clamped.Page);
            Assert.Equal(PaginationQuery.MaxPageSize, clamped.PageSize);

            var search = await GetAsync<PagedResult<GetCategoryItemDto>>("/api/categories?search=Category 0");
            Assert.Equal(9, search.TotalCount);
        }
    }
}
