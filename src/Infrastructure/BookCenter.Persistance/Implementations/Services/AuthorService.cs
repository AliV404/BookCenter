using AutoMapper;
using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.Authors;
using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Persistance.Implementations.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IMapper _mapper;
        private readonly IAuthorRepository _authorRepository;

        public AuthorService(IMapper mapper, IAuthorRepository authorRepository)
        {
            _mapper = mapper;
            _authorRepository = authorRepository;
        }
        public async Task<IEnumerable<GetAuthorItemDto>> GetAllAsync(int page, int take)
        {
            return _mapper.Map<IEnumerable<GetAuthorItemDto>>(await _authorRepository.GetAll(skip: (page - 1) * take, take: take).ToListAsync());
        }

        public async Task<GetAuthorDto> GetByIdAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id, "Books");
            if (author == null)
                throw new Exception("Null");

            return _mapper.Map<GetAuthorDto>(author);
        }

        public async Task CreateAsync(CreateAuthorDto authorDto)
        {
            if (await _authorRepository.AnyAsync(a => a.Name == authorDto.Name))
                throw new Exception("Exists!");
            await _authorRepository.AddAsync(_mapper.Map<Author>(authorDto));
            await _authorRepository.SaveChangesAsync();
        }
        public async Task UpdateAsync(int id, UpdateAuthorDto authorDto)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
                throw new Exception("Null");

            if (await _authorRepository.AnyAsync(a => a.Name == authorDto.Name && a.Id != id))
                throw new Exception("Exists!");

            _authorRepository.Update(_mapper.Map(authorDto, author));
            await _authorRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
                throw new Exception("Null");
            _authorRepository.Delete(author);
            await _authorRepository.SaveChangesAsync();
        }
    }
}
