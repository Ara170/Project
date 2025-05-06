using Microsoft.EntityFrameworkCore;
using HotelManagementAPI.Models;

namespace HotelManagementAPI.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options)
        {
        }

        public DbSet<TypeRoom> TypeRooms { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<DetailBooking> DetailBookings { get; set; }
        public DbSet<DetailService> DetailServices { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite key for DetailService
            modelBuilder.Entity<DetailService>()
                .HasKey(ds => new { ds.Booking_Service_ID, ds.ServiceID });

            // Configure composite key for Feedback
            modelBuilder.Entity<Feedback>()
                .HasKey(f => new { f.UserID, f.TimeComment });

            // Configure relationships and constraints
            modelBuilder.Entity<Room>()
                .Property(r => r.State)
                .HasConversion<string>()
                .HasMaxLength(50);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.TypeRoom)
                .WithMany(tr => tr.Rooms)
                .HasForeignKey(r => r.RoomType);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserID);

            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithOne(u => u.Staff)
                .HasForeignKey<Staff>(s => s.UserID);

            // Additional configurations
            base.OnModelCreating(modelBuilder);
        }
    }
}
