using System.Net.Sockets;
using Microsoft.EntityFrameworkCore; // מייבא EF Core
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApplication2.Models; // מייבא מודלים

namespace WebApplication2.DAL // מרחב שמות ל-DAL
{ // התחלת namespace
    public class StoreContext : DbContext // DbContext של האפליקציה
    { // התחלת מחלקה
        public StoreContext(DbContextOptions<StoreContext> options) : base(options) { } // בנאי המקבל אופציות

        public DbSet<UserModel> Users { get; set; } // DbSet למשתמשים
        public DbSet<DonorModel> Donors { get; set; } // DbSet לתורמים
        public DbSet<GiftModel> Gifts { get; set; } // DbSet למתנות
        public DbSet<CategoryModel> Categories { get; set; } // DbSet לקטגוריות

        public DbSet<OrderModel> Orders { get; set; } // DbSet להזמנות

        public DbSet<OrderTicketModel> OrderTicket { get; set; } // DbSet לפרטי הזמנה

        public DbSet<WinnerModel> Winners { get; set; } // DbSet לזוכים
        public DbSet<TicketModel> Tickets { get; set; } // DbSet לכרטיסים

        protected override void OnModelCreating(ModelBuilder modelBuilder) // קונפיגורציה של מודלים
        { // התחלת שיטה
            // --- Value converter for UserRole enum -> string in DB ---
            var userRoleConverter = new ValueConverter<UserRole, string>(
                v => v.ToString(),
                v => Enum.Parse<UserRole>(v));

            // --- User entity ---
            modelBuilder.Entity<UserModel>(b =>
            {
                b.HasKey(u => u.Id);

                // Unique index on Email
                b.HasIndex(u => u.Email).IsUnique();

                // Map Role enum to string column
                b.Property(u => u.Role)
                 .HasConversion(userRoleConverter)
                 .HasColumnType("nvarchar(50)")
                 .IsRequired();

                // Global query filter for soft delete
                b.HasQueryFilter(u => !u.IsDeleted);
            });

            // --- Category entity ---
            modelBuilder.Entity<CategoryModel>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Name).IsRequired();
                b.HasQueryFilter(c => !c.IsDeleted);
            });

            // --- Donor entity ---
            modelBuilder.Entity<DonorModel>(b =>
            {
                b.HasKey(d => d.Id);
                b.Property(d => d.Name).IsRequired();
                b.Property(d => d.Email).IsRequired();
                b.HasQueryFilter(d => !d.IsDeleted);
            });

            // --- Gift entity ---
            modelBuilder.Entity<GiftModel>(b =>
            {
                b.HasKey(g => g.Id);

                b.Property(g => g.TicketPrice)
                    .HasColumnType("decimal(18,2)");

                b.Property(g => g.Name).IsRequired();

                b.HasOne(g => g.Donor)
                    .WithMany(d => d.Gifts)
                    .HasForeignKey(g => g.DonorId);

                b.HasOne(g => g.Category)
                    .WithMany(c => c.Gifts)
                    .HasForeignKey(g => g.CategoryId);

                b.HasQueryFilter(g => !g.IsDeleted);
            });
            // --- Order -> User relationship (עם הגנה על חוסר עקביות נתונים) ---
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); // מניעת מחיקת משתמש אם קיימות הזמנות

            // --- OrderTicket / Order / Winner navigations and other entity config kept as before ---
            modelBuilder.Entity<OrderTicketModel>() // קשר OrderItem -> Order
                .HasOne(oi => oi.Order) // לכל פריט יש הזמנה
                .WithMany(o => o.OrderItems) // להזמנה יש פריטים רבים
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // מחיקת פריטים כשמוחקים הזמנה

            // --- OrderTicket -> Gift relationship (עם הגנה על חוסר עקביות נתונים) ---
            modelBuilder.Entity<OrderTicketModel>()
                .HasOne(oi => oi.Gift)
                .WithMany()
                .HasForeignKey(oi => oi.GiftId)
                .OnDelete(DeleteBehavior.Restrict); // מניעת מחיקת מתנה אם קיימת בהזמנה

            modelBuilder.Entity<WinnerModel>().HasKey(w => w.Id);

            // קשר זוכה -> מתנה (1 ל-1 או 1 ל-רבים תלוי בלוגיקה, כאן נגדיר שלכל זכייה יש מתנה אחת)
            modelBuilder.Entity<WinnerModel>()
                .HasOne(w => w.Gift) // אובייקט המתנה בתוך מודל הזוכה
                .WithMany()            // למתנה יכולים להיות מספר זוכים (למשל בהגרלות שונות)
                .HasForeignKey(w => w.GiftId)
                .OnDelete(DeleteBehavior.Restrict); // מניעת מחיקה משורשרת כדי לשמור על היסטוריה

            // קשר זוכה -> משתמש/רוכש
            modelBuilder.Entity<WinnerModel>()
                .HasOne(w => w.User)   // המשתמש שזכה
                .WithMany()            // למשתמש יכולות להיות מספר זכיות
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        
    } // סיום שיטה
    } // סיום מחלקה
} // סיום namespace