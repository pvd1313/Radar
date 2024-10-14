using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;

namespace Beast.Claw.Reflection
{
    public static class TypeStorage<T>
    {
        public static readonly string FullName = typeof(T).FullName;

        private static readonly Dictionary<string, string> _fullNameWithPostfix;
            
        static TypeStorage()
        {
            try
            {
                FullName = typeof(T).FullName;
                _fullNameWithPostfix = new Dictionary<string, string>();
            }
            catch (Exception e)
            {
                BDebug.StaticLogException(e);
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