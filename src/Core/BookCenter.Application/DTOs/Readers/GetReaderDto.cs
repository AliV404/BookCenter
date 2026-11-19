namespace BookCenter.Application.DTOs.Readers
{
    /// <param name="HasAccount">True when the reader signs in online; their email is then also their login.</param>
    public record GetReaderDto(int Id, string Name, string Surname, string Email, string Phone, int ActiveLoans, bool HasAccount);
}
