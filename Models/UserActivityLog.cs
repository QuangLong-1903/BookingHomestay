using System;
using System.ComponentModel.DataAnnotations;

namespace DoAnCoSo_Nhom2.Models
{
    public class UserActivityLog
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }

        public User User { get; set; }
    }
}