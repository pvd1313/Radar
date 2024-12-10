using System;
using System.Collections.Generic;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Common;
using Beast.Claw.Debug;

namespace Beast.Claw.Reflection
{
    [StaticUtility]
    public static class BEnum
    {
        private static readonly Dictionary<Type, EnumData> ENUMS;
        private static readonly Type FLAGS_ATTRIBUTE_TYPE;

        static BEnum()
        {
            try
            {
                ENUMS = new Dictionary<Type, EnumData>(64);
                FLAGS_ATTRIBUTE_TYPE = typeof(FlagsAttribute);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        public static TEnum[] GetValues<TEnum>()
        {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }

        public static bool TryGetEnumData(Type type, out EnumData enumData)
        {
            enumData = default;
            
            if (!type.IsEnum)
            {
                return false;
            }

            enumData = ENUMS.GetOrCreateWithFactory(type, _parseEnum);

            return true;
        }

        private static EnumData _parseEnum(Type type)
        {
            bool isFlags = false;

            foreach (AttributeData attribute in BReflect.GetCustomAttributes(type))
            {
                if (attribute.type.Implements(FLAGS_ATTRIBUTE_TYPE))
                {
                    isFlags = true;
                    break;
                }
            }

            Array values = type.GetEnumValues();

            TypeCode typeCode = Type.GetTypeCode(Enum.GetUnderlyingType(type));

            if (isFlags)
            {
                long mask = 0;

                for (int i = 0; i < values.Length; i++)
                {
                    mask |= BReflect.UnboxAsLong(values.GetValue(i), typeCode);
                }

                return new EnumData
                {
                    isFlags = true,
                    flagsMask = mask,
                    typeCode = typeCode
                };
            }

            HashSet<long> allValues = new (values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                object value = values.GetValue(i);

                long asLong = BReflect.UnboxAsLong(value, typeCode);
                
                allValues.Add(asLong);
            }

            return new EnumData
            {
                isFlags = false,
                allValues = allValues,
                typeCode = typeCode
            };
        }
    }
}