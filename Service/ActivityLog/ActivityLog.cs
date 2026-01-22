using DoAnCoSo_Nhom2.Data;
using DoAnCoSo_Nhom2.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DoAnCoSo_Nhom2.Services.ActivityLog
{
    public class ActivityLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public ActivityLogService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task LogAsync(string userId, string action)
        {
            var timeZoneId = _configuration["TimeZoneId"] ?? "SE Asia Standard Time";
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var timestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

            var log = new UserActivityLog
            {
                UserId = userId,
                Action = action,
                Timestamp = timestamp
            };

            _context.UserActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
