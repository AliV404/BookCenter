using AutoMapper;
using BookCenter.Application.DTOs.Categories;
using BookCenter.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Application.MappingProfiles
{
    public class CategoryProfile:Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, GetCategoyDto>();
            CreateMap<Category, GetCategoyItemDto>();

            CreateMap<CreateCategoyDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
        }
    }
}
