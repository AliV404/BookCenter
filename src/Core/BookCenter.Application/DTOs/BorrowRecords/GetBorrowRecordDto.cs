namespace BookCenter.Application.DTOs.BorrowRecords
{
    public record GetBorrowRecordDto(
        int Id,
        int BookId,
        string BookName,
        int ReaderId,
        string ReaderFullName,
        DateTime BorrowedAt,
        DateTime DueDate,
        DateTime? ReturnedAt,
        bool IsOverdue);
}
