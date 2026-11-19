using BookCenter.Application.DTOs.Books;

namespace BookCenter.Application.DTOs.Categories
{
    public record GetCategoryDto(int Id, string Name, IEnumerable<GetBookItemDto> Books);
}
