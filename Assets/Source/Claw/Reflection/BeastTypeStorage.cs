using System;
using System.Collections.Generic;
using Beast.Claw.Common;

namespace Beast.Claw.Reflection
{
    public static class BeastTypeStorage<T>
    {
        public static readonly string FullName = typeof(T).FullName;

        private static readonly Dictionary<string, string> _fullNameWithPostfix;
            
        static BeastTypeStorage()
        {
            try
            {
                FullName = typeof(T).FullName;
                _fullNameWithPostfix = new Dictionary<string, string>();
            }
            catch (Exception e)
            {
                BeastLog.StaticException(e, "TypeNameStorage<T> cctor");
            }
        }

        public static string FullNameWithPostfixOrFullName(string postfix)
        {
            if (string.IsNullOrEmpty(postfix))
            {
                return FullName;
            }
            
            if (_fullNameWithPostfix.TryGetValue(postfix, out string text))
            {
                return text;
            }

            text = FullName + "_" + postfix;

            _fullNameWithPostfix[postfix] = text;

            return text;
        }
            
    }
}