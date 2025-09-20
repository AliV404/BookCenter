using AutoMapper;
using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Application.DTOs.Authors;
using BookCenter.Application.DTOs.Publishers;
using BookCenter.Domain.Entities;
using BookCenter.Persistance.Implementations.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookCenter.Persistance.Implementations.Services
{
    public class PublisherService : IPublihserService
    {
        private readonly IMapper _mapper;
        private readonly IPublisherRepository _publisherRepository;

        public PublisherService(IMapper mapper, IPublisherRepository publisherRepository)
        {
            _mapper = mapper;
            _publisherRepository = publisherRepository;
        }

        public async Task<IEnumerable<GetPublisherItemDto>> GetAllAsync(int page, int take)
        {
            return _mapper.Map<IEnumerable<GetPublisherItemDto>>(await _publisherRepository.GetAll(skip: (page - 1) * take, take: take).ToListAsync());
        }

        public async Task<GetPublisherDto> GetByIdAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id, "Books");
            if (publisher == null)
                throw new Exception("Null");

            return _mapper.Map<GetPublisherDto>(publisher);
        }

        public async Task CreateAsync(CreatePublisherDto publisherDto)
        {
            if (await _publisherRepository.AnyAsync(a => a.Name == publisherDto.Name))
                throw new Exception("Exists!");
            await _publisherRepository.AddAsync(_mapper.Map<Publisher>(publisherDto));
            await _publisherRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id, "Books");
            if (publisher == null)
                throw new Exception("Null");

            _publisherRepository.Delete(publisher);
            await _publisherRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(int id, UpdatePublisherDto publisherDto)
        {
            var publisher = await _publisherRepository.GetByIdAsync(id, "Books");
            if (publisher == null)
                throw new Exception("Null");

            if (await _publisherRepository.AnyAsync(a => a.Name == publisherDto.Name && a.Id != id))
                throw new Exception("Exists!");

            _publisherRepository.Update(_mapper.Map(publisherDto, publisher));
            await _publisherRepository.SaveChangesAsync();
        }
    }
}
