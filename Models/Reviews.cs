namespace DoAnCoSo_Nhom2.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int HomestayId { get; set; }
        public Homestay Homestay { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } 
        public DateTime CreatedAt { get; set; }
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}