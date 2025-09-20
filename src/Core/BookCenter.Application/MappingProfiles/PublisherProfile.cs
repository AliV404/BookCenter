using AutoMapper;
using BookCenter.Application.DTOs.Publishers;
using BookCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.MappingProfiles
{
    internal class PublisherProfile:Profile
    {
        public PublisherProfile()
        {
            CreateMap<Publisher, GetPublisherItemDto>();
            CreateMap<Publisher, GetPublisherDto>();
            CreateMap<CreatePublisherDto, Publisher>();
            CreateMap<UpdatePublisherDto, Publisher>();
        }
    }
}
