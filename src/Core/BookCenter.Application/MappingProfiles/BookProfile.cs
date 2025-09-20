using AutoMapper;
using BookCenter.Application.DTOs.Authors;
using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.Publishers;
using BookCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.MappingProfiles
{
    public class BookProfile:Profile
    {
        public BookProfile()
        {
            CreateMap<Book, GetBookItemDto>();
            CreateMap<Book, GetBookDto>();
            CreateMap<CreateBookDto, Book>();
            CreateMap<UpdateBookDto, Book>().ForMember(dest=> dest.Id, opt=> opt.Ignore());

        }
    }
}
