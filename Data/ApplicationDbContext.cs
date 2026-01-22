using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_Nhom2.Models;
using System.Text.Json;

namespace DoAnCoSo_Nhom2.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Homestay> Homestays { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<HomestayAmenity> HomestayAmenities { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }

        public DbSet<Report> Reports { get; set; }
        public DbSet<DeletedHomestayNotification> DeletedHomestayNotifications { get; set; }
        public DbSet<TempBooking> TempBookings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<HomestayAmenity>()
                .HasKey(ha => new { ha.HomestayId, ha.AmenityId });

            modelBuilder.Entity<HomestayAmenity>()
                .HasOne(ha => ha.Homestay)
                .WithMany(h => h.HomestayAmenities)
                .HasForeignKey(ha => ha.HomestayId);

            modelBuilder.Entity<HomestayAmenity>()
                .HasOne(ha => ha.Amenity)
                .WithMany(a => a.HomestayAmenities)
                .HasForeignKey(ha => ha.AmenityId);

            modelBuilder.Entity<Booking>()
                 .HasOne(b => b.Homestay)
                  .WithMany(h => h.Bookings)
                  .HasForeignKey(b => b.HomestayId)
                  .IsRequired();

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .IsRequired();

            modelBuilder.Entity<Booking>()
                .Property(b => b.Status)
                .IsRequired();

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Homestay)
                .WithMany(h => h.Reviews)
                .HasForeignKey(r => r.HomestayId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithMany()
                .HasForeignKey(p => p.BookingId);

            modelBuilder.Entity<Homestay>()
                .Property(h => h.Images)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));

            modelBuilder.Entity<Amenity>().HasData(
                new Amenity { Id = 1, Name = "Wi-Fi" },
                new Amenity { Id = 2, Name = "Swimming Pool" },
                new Amenity { Id = 3, Name = "Parking" },
                new Amenity { Id = 4, Name = "Bida" },
                new Amenity { Id = 5, Name = "Phòng karaoke" }
            );

            modelBuilder.Entity<HomestayAmenity>().HasData(
                new HomestayAmenity { HomestayId = 1, AmenityId = 1 },
                new HomestayAmenity { HomestayId = 1, AmenityId = 2 }
            );
        }
    }
}