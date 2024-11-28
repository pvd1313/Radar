using System;
using System.Collections.Generic;
using Beast.Claw.Debug;

namespace Beast.Claw.Reflection
{
    public static class TypeStorage<T>
    {
        public static readonly string FULL_NAME = typeof(T).FullName;

        private static readonly Dictionary<string, string> FULL_NAME_WITH_POSTFIX;
            
        static TypeStorage()
        {
            try
            {
                FULL_NAME = typeof(T).FullName;
                FULL_NAME_WITH_POSTFIX = new Dictionary<string, string>();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        public static string FullNameWithPostfixOrFullName(string postfix)
        {
            if (string.IsNullOrEmpty(postfix))
            {
                return FULL_NAME;
            }
            
            if (FULL_NAME_WITH_POSTFIX.TryGetValue(postfix, out string text))
            {
                return text;
            }

            text = FULL_NAME + "_" + postfix;

            FULL_NAME_WITH_POSTFIX[postfix] = text;

            return text;
        }
    }
}