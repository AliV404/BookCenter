using BookCenter.Application.Abstractions.Repositories;
using BookCenter.Application.Abstractions.Services;
using BookCenter.Persistence.Data;
using BookCenter.Persistence.Implementations.Repositories;
using BookCenter.Persistence.Implementations.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookCenter.Persistence.ServiceRegistration
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(configuration.GetConnectionString("Default")));

            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IPublisherRepository, PublisherRepository>();
            services.AddScoped<IReaderRepository, ReaderRepository>();
            services.AddScoped<IBorrowRecordRepository, BorrowRecordRepository>();

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IAuthorService, AuthorService>();
            services.AddScoped<IPublisherService, PublisherService>();
            services.AddScoped<IReaderService, ReaderService>();
            services.AddScoped<IBorrowRecordService, BorrowRecordService>();

            return services;
        }
    }
}
