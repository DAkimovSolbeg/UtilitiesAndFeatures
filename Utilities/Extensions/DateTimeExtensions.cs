using System;
using TimeZoneConverter;

namespace Utilities.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime ConvertDateTimeFromUtc(this DateTime dateTime, string timezone)
        {
            if (string.IsNullOrWhiteSpace(timezone))
            {
                throw new ArgumentException($"{nameof(timezone)} parameter is invalid.");
            }

            var timezoneInfo = TZConvert.GetTimeZoneInfo(timezone);
            return DateTime.SpecifyKind(
                TimeZoneInfo.ConvertTimeFromUtc(dateTime, timezoneInfo),
                DateTimeKind.Utc);
        }
    }
}
