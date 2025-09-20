using AutoMapper;
using BookCenter.Application.DTOs.Books;
using BookCenter.Application.DTOs.BorrowRecords;
using BookCenter.Application.DTOs.Readers;
using BookCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.MappingProfiles
{
    public class BorrowRecordProfile:Profile
    {
        public BorrowRecordProfile()
        {
            CreateMap<BorrowRecord, GetBorrowRecordDto>()
                .ForMember(dest=> dest.Book, opt=> opt.MapFrom(src=> src.Book))
                .ForMember(dest=> dest.Reader, opt=> opt.MapFrom(src=> src.Reader));


            CreateMap<CreateBorrowRecordDto, BorrowRecord>();
            CreateMap<UpdateBorrowRecordDto, BorrowRecord>();

            CreateMap<Reader, GetReaderItemDto>();
        }
    }
}
