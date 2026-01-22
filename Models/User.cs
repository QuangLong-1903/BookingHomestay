using Microsoft.AspNetCore.Identity;

namespace DoAnCoSo_Nhom2.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string PhoneNumber { get; set; }

    }
}