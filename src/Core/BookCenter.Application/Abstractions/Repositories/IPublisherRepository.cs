using BookCenter.Application.Abstractions.Repositories.Generic;
using BookCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.Abstractions.Repositories
{
    public interface IPublisherRepository:IRepository<Publisher>
    {
    }
}
