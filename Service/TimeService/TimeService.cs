namespace DoAnCoSo_Nhom2.Service.TimeService
{
    public class TimeService
    {
        private readonly TimeZoneInfo _timeZone;

        public TimeService(IConfiguration configuration)
        {
            var timeZoneId = configuration["TimeZoneId"] ?? "SE Asia Standard Time";
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }

        public DateTime Now()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, _timeZone);
        }
    }
}
