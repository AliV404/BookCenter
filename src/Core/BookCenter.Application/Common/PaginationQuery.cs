namespace BookCenter.Application.Common
{
    /// <summary>
    /// Query-string parameters shared by every list endpoint.
    /// Out-of-range values are clamped instead of rejected, so a bad page number never causes a 500.
    /// </summary>
    public class PaginationQuery
    {
        public const int MaxPageSize = 100;

        private int _page = 1;
        private int _pageSize = 10;
        private string? _search;

        public int Page
        {
            get => _page;
            set => _page = Math.Max(1, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Clamp(value, 1, MaxPageSize);
        }

        public string? Search
        {
            get => _search;
            set => _search = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }

    public class BookQuery : PaginationQuery
    {
        public int? AuthorId { get; set; }
        public int? CategoryId { get; set; }
        public int? PublisherId { get; set; }
    }

    public enum BorrowStatus
    {
        All,
        Active,
        Overdue,
        Returned
    }

    public class BorrowRecordQuery : PaginationQuery
    {
        public int? ReaderId { get; set; }
        public int? BookId { get; set; }
        public BorrowStatus Status { get; set; } = BorrowStatus.All;
    }
}
