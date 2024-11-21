using System;
using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Common
{
    [ExtensionClass]
    public static class DateTimeExtension
    {
        public static TimeSpan GetTimePassedFrom(this DateTime now, DateTime past)
        {
            return now - past;
        }
    }
}