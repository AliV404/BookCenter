using BookCenter.Application.DTOs.Authors;
using BookCenter.Application.DTOs.Categories;
using BookCenter.Application.DTOs.Publishers;
using BookCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.Books
{
    public record GetBookDto
        (
        int Id,
        string Name,
        string ISBN,
        int PublishYear,
        int CopiesAvailable,
        int AuthorId,
        GetAuthorItemDto Author,
        int CategoryId,
        GetCategoyItemDto Category,
        int PublisherId,
        GetPublisherItemDto Publisher
        );
}
