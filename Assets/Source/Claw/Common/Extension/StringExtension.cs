using System;
using System.Text;
using Beast.Claw.Collection;
using Beast.Claw.Debug;

namespace Beast.Claw.Common
{
    public static class StringExtension
    {
        private static readonly ManagedRingPool<StringBuilder> BUILDERS; 
        
        static StringExtension()
        {
            try
            {
                BUILDERS = new ManagedRingPool<StringBuilder>(16);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
        /// <returns>'text'</returns>
        public static string Q(this string text)
        {
            StringBuilder builder = BUILDERS.Next();
            
            builder.Clear();
            builder.Append("'");
            builder.Append(text);
            builder.Append("'");

            return builder.ToString();
        }
        
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }
    }
}