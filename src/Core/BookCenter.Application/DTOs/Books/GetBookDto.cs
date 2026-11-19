namespace BookCenter.Application.DTOs.Books
{
    public record GetBookDto(
        int Id,
        string Name,
        string ISBN,
        int PublishYear,
        int TotalCopies,
        int CopiesAvailable,
        int AuthorId,
        string AuthorName,
        int CategoryId,
        string CategoryName,
        int PublisherId,
        string PublisherName);
}
