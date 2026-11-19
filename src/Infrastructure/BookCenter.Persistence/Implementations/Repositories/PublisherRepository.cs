using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Data;
using BookCenter.Persistence.Implementations.Repositories.Generic;

namespace BookCenter.Persistence.Implementations.Repositories
{
    public class PublisherRepository : Repository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(AppDbContext context) : base(context) { }
    }
}
