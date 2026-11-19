namespace BookCenter.Application.DTOs.Readers
{
    public record GetReaderDto(int Id, string Name, string Surname, string Email, string Phone, int ActiveLoans);
}
