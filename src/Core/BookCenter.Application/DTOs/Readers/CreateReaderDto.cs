namespace BookCenter.Application.DTOs.Readers
{
    public class CreateReaderDto
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
}
