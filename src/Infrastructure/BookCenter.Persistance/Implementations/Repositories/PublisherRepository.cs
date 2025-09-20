using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Domain.Entities;
using BookCenter.Persistance.Data;
using BookCenter.Persistance.Implementations.Repositories.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Persistance.Implementations.Repositories
{
    public class PublisherRepository:Repository<Publisher>, IPublisherRepository
    {
        public PublisherRepository(AppDbContext context) : base(context) { }

    }
}
