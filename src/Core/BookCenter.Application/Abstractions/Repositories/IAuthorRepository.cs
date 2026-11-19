using BookCenter.Application.Abstractions.Repositories.Generic;
using BookCenter.Domain.Entities;

namespace BookCenter.Application.Abstractions.Repositories
{
    public interface IAuthorRepository : IRepository<Author>
    {
    }
}
