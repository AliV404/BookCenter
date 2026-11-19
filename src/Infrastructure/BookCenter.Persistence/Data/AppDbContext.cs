using BookCenter.Domain.Base;
using BookCenter.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;
using System.Reflection;

namespace BookCenter.Persistence.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                    entityType.FindProperty(auditColumn)?.SetMaxLength(100);
            }

            base.OnModelCreating(modelBuilder);
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

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = now;
                        break;

                    case EntityState.Deleted:
                        // Turn a physical DELETE into a soft delete: keep the row, flag it.
                        // This keeps borrow history intact and works with the query filter above.
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
