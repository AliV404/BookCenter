namespace BookCenter.Application.DTOs.Books
{
    public class CreateBookDto
    {
        public string Name { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public int PublishYear { get; set; }
        public int TotalCopies { get; set; }
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        public int PublisherId { get; set; }
    }
}
