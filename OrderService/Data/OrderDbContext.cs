using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderTicketModel> OrderTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderModel>(b =>
            {
                b.HasKey(o => o.Id);
                b.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<OrderTicketModel>(b =>
            {
                b.HasKey(t => t.Id);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
