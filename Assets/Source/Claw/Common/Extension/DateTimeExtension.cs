using System;

namespace Beast.Claw.Common
{
    public static class DateTimeExtension
    {
        public static TimeSpan GetTimePassedFrom(this DateTime now, DateTime past)
        {
            return now - past;
        }
    }
}