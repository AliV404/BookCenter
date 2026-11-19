using BookCenter.Application.DTOs.Books;

namespace BookCenter.Application.DTOs.Authors
{
    public record GetAuthorDto(int Id, string Name, string? Biography, IEnumerable<GetBookItemDto> Books);
}
