namespace DoAnCoSo_Nhom2.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int HomestayId { get; set; }
        public Homestay Homestay { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public DateTime CheckInDate { get; set; } 
        public DateTime CheckOutDate { get; set; } 
        public int NumberOfGuests { get; set; } 
        public decimal TotalPrice { get; set; } 
        public string Status { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}