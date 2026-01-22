namespace DoAnCoSo_Nhom2.Models
{
    public class TempBooking
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int HomestayId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
