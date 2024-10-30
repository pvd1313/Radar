using System;
using System.Collections.Generic;
using Beast.Claw.Debug;

using Attribute = System.Attribute;

namespace Beast.Claw.Reflection
{
    public static class TypeStorage<T>
    {
        public static readonly string FULL_NAME = typeof(T).FullName;

        private static readonly Dictionary<string, string> FULL_NAME_WITH_POSTFIX;
        private static readonly Dictionary<Type, bool> ATTRIBUTE_CHECKS;
            
        static TypeStorage()
        {
            try
            {
                FULL_NAME = typeof(T).FullName;
                FULL_NAME_WITH_POSTFIX = new Dictionary<string, string>();
                ATTRIBUTE_CHECKS = new Dictionary<Type, bool>();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        public static bool HasAttribute<TAttribute>() where TAttribute : Attribute
        {
            Type attributeType = typeof(TAttribute);

            if (ATTRIBUTE_CHECKS.TryGetValue(attributeType, out bool result))
            {
                return result;
            }

            result = typeof(T).GetCustomAttributes(typeof(TAttribute), true).Length > 0;

            ATTRIBUTE_CHECKS[attributeType] = result;

            return result;
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