using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace DoAnCoSo_Nhom2.Models
{
    public class Homestay
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public decimal PricePerNight { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public List<HomestayAmenity> HomestayAmenities { get; set; } = new List<HomestayAmenity>();
        public List<Review> Reviews { get; set; } = new List<Review>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();

        [NotMapped]
        public double Rating => Reviews.Any() ? Reviews.Average(r => r.Rating) : 0;

        public string? StaffId { get; set; }
        [ForeignKey("StaffId")]
        public virtual User? Staff { get; set; }
        public bool Free { get; set; } = true;
        public bool IsApproved { get; set; }

    }
}