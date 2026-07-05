using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AuthenticationService.Models;

namespace AuthenticationService.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<UserModel> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var userRoleConverter = new ValueConverter<UserRole, string>(
                v => v.ToString(),
                v => Enum.Parse<UserRole>(v));

            modelBuilder.Entity<UserModel>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Role)
                    .HasConversion(userRoleConverter)
                    .HasColumnType("nvarchar(50)")
                    .IsRequired();
                b.HasQueryFilter(u => !u.IsDeleted);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
