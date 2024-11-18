using System;
using Beast.Claw.Collection;
using Beast.Claw.Debug;

namespace Beast.Claw.Text
{
    public static class StringExtension
    {
        private static readonly RingPool<StringBuilder> BUILDERS; 
        
        static StringExtension()
        {
            try
            {
                BUILDERS = new RingPool<StringBuilder>(16, () => new StringBuilder(256));
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
        /// <returns>'text'</returns>
        public static string Q(this string text)
        {
            StringBuilder builder = BUILDERS.Next().Clear();
            builder.Add('\'');
            builder.Add(text);
            builder.Add('\'');

            return builder.ToString();
        }

        /// <returns>'obj.ToString()'</returns>
        public static string Q(this object obj)
        {
            StringBuilder builder = BUILDERS.Next().Clear();
            builder.Add('\'');
            builder.Add(obj.ToString());
            builder.Add('\'');

            return builder.ToString();
        }
        
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }
    }
}