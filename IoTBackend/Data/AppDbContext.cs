using IoTBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Reading> Readings { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reading>(entity =>
            {
                entity.HasKey(e => e.ReadingId);
                entity.Property(e => e.Timestamp).IsRequired();
            });
        }
    }
}
