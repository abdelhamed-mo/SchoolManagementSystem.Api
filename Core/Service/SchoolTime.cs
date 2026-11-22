namespace Service
{
    public static class SchoolTime
    {
        public static TimeZoneInfo CairoTimeZone
        {
            get
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                }
                catch (TimeZoneNotFoundException)
                {
                    return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                }
            }
        }

        public static DateTime TodayInCairo()
            => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CairoTimeZone).Date;

        public static DateTime SundayOfWeek(DateTime date)
        {
            var normalized = date.Date;
            return normalized.AddDays(-(int)normalized.DayOfWeek);
        }
    }
}
