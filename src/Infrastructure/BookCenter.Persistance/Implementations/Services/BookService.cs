using AutoMapper;
using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.Books;
using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookCenter.Persistance.Implementations.Services
{
    public class BookService : IBookService
    {
        private readonly IMapper _mapper;
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;

        public BookService(IMapper mapper, IBookRepository bookRepository, ICategoryRepository categoryRepository)
        {
            _mapper = mapper;
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<IEnumerable<GetBookItemDto>> GetAllAsync(int page, int take)
        {
            var books = _mapper.Map<IEnumerable<GetBookItemDto>>(await _bookRepository.GetAll(skip:(page-1)*take, take: take).ToListAsync());
            return books;
        }

        public async Task<GetBookDto> GetByIdAsync(int id)
        {
            var book = _mapper.Map<GetBookDto>(await _bookRepository.GetByIdAsync(id, "Category", "Publisher", "Author"));
            if (book == null) throw new Exception("Book is not found");
            return book;
        }

        public async Task CreateAsync(CreateBookDto dto)
        {
            if (!await _categoryRepository.AnyAsync(c => c.Id == dto.CategoryId))
                throw new Exception("Category doesn't exists!");
            await _bookRepository.AddAsync(_mapper.Map<Book>(dto));
            await _bookRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, UpdateBookDto dto)
        {
            var book = await _bookRepository.GetByIdAsync(id, "Category", "Publisher", "Author");
            if (dto.CategoryId != book.CategoryId)
                if (!await _categoryRepository.AnyAsync(c => c.Id == dto.CategoryId))
                    throw new Exception("Category doesn't exists!");

            _bookRepository.Update(_mapper.Map(dto, book));
            await _bookRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) throw new Exception("Not found");

            _bookRepository.Delete(book);
            await _bookRepository.SaveChangesAsync();
        }
    }
}
