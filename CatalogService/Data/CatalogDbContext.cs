using Microsoft.EntityFrameworkCore;
using CatalogService.Models;

namespace CatalogService.Data
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

        public DbSet<CategoryModel> Categories => Set<CategoryModel>();
        public DbSet<DonorModel> Donors => Set<DonorModel>();
        public DbSet<GiftModel> Gifts => Set<GiftModel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CategoryModel>()
                .HasMany(c => c.Gifts)
                .WithOne(g => g.Category)
                .HasForeignKey(g => g.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DonorModel>()
                .HasMany(d => d.Gifts)
                .WithOne(g => g.Donor)
                .HasForeignKey(g => g.DonorId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
