using AutoMapper;
using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.Categories;
using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Persistance.Implementations.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GetCategoyItemDto>> GetAllAsync(int page, int take)
        {
            return _mapper.Map<IEnumerable<GetCategoyItemDto>>
                (await _categoryRepository.GetAll(skip: (page - 1) * take, take: take).ToListAsync());
        }

        public async Task<GetCategoyDto> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id, "Books");
            if (category == null) throw new Exception("Null");

            return _mapper.Map<GetCategoyDto>(category);
        }
        public async Task CreateAsync(CreateCategoyDto dto)
        {
            if (await _categoryRepository.AnyAsync(c => c.Name == dto.Name))
                throw new Exception("Exsist");
            await _categoryRepository.AddAsync(_mapper.Map<Category>(dto));
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            if (category == null) throw new Exception("Not found");


            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();
        }
        public async Task UpdateAsync(int id, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                if (category == null) throw new Exception("Not found");

            if(await _categoryRepository.AnyAsync(c=> c.Name == dto.Name && c.Id != id))
                throw new Exception("Already Exists!");

            _categoryRepository.Update(_mapper.Map(dto, category));
            await _categoryRepository.SaveChangesAsync();
        }
    }
}
