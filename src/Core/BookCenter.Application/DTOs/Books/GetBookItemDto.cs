namespace BookCenter.Application.DTOs.Books
{
    /// <summary>Short book reference, used inside author / category / publisher details.</summary>
    public record GetBookItemDto(int Id, string Name);
}
