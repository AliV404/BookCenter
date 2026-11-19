using BookCenter.Application.Abstractions.Security;
using BookCenter.Domain.Base;
using BookCenter.Domain.Entities;
using BookCenter.Persistence.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;
using System.Reflection;

namespace BookCenter.Persistence.Data
{
    /// <summary>Library data plus the ASP.NET Core Identity tables (AspNetUsers, AspNetRoles, ...).</summary>
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        private readonly ICurrentUser _currentUser;

        public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser) : base(options)
        {
            _currentUser = currentUser;
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity's own table configuration must run first.
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
            {
                // Global soft-delete filter: every query automatically gets "WHERE IsDeleted = 0".
                // Use IgnoreQueryFilters() when you really need deleted rows (e.g. an admin "trash" page).
                var e = Expression.Parameter(entityType.ClrType, "e");
                var notDeleted = Expression.Not(Expression.Property(e, nameof(BaseEntity.IsDeleted)));
                entityType.SetQueryFilter(Expression.Lambda(notDeleted, e));

                foreach (var auditColumn in new[] { nameof(BaseEntity.CreatedBy), nameof(BaseEntity.ModifiedBy), nameof(BaseEntity.DeletedBy) })
                    entityType.FindProperty(auditColumn)?.SetMaxLength(256); // holds the user's email
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            // All dates are stored as UTC. SQL Server's datetime2 loses DateTimeKind, so without this
            // values come back as "Unspecified" and are serialized without a 'Z' — browsers would then
            // misread them as local time.
            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
            configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
        }

        private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
        {
            public UtcDateTimeConverter() : base(
                toDb => toDb.Kind == DateTimeKind.Local ? toDb.ToUniversalTime() : DateTime.SpecifyKind(toDb, DateTimeKind.Utc),
                fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc))
            { }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var who = _currentUser.Email; // null for anonymous requests and startup seeding

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatedBy = who;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = now;
                        entry.Entity.ModifiedBy = who;
                        break;

                    case EntityState.Deleted:
                        // Turn a physical DELETE into a soft delete: keep the row, flag it.
                        // This keeps borrow history intact and works with the query filter above.
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeletedBy = who;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
