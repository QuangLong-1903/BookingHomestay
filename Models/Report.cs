namespace DoAnCoSo_Nhom2.Models
{
    public class Report
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public string Reason { get; set; }
        public DateTime ReportedAt { get; set; }
        public string? Status { get; set; }
        public User User { get; set; }
        public Review Review { get; set; }
    }
}
