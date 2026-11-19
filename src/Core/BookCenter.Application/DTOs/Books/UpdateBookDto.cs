namespace BookCenter.Application.DTOs.Books
{
    public class UpdateBookDto
    {
        public string Name { get; set; } = null!;
        public string ISBN { get; set; } = null!;
        public int PublishYear { get; set; }

        /// <summary>
        /// New total stock. Available copies are recalculated from it;
        /// it cannot go below the number of copies currently on loan.
        /// </summary>
        public int TotalCopies { get; set; }
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        public int PublisherId { get; set; }
    }
}
