using BookCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.DTOs.Books
{
    public class CreateBookDto
    {
        public string Name { get; set; }
        public string ISBN { get; set; }
        public int PublishYear { get; set; }
        public int CopiesAvailable { get; set; }
        public int AuthorId { get; set; }
        public int CategoryId { get; set; }
        public int PublisherId { get; set; }
    }
}
