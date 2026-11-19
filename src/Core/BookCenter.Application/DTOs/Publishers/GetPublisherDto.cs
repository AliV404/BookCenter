using BookCenter.Application.DTOs.Books;

namespace BookCenter.Application.DTOs.Publishers
{
    public record GetPublisherDto(int Id, string Name, IEnumerable<GetBookItemDto> Books);
}
