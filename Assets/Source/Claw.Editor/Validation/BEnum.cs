using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Jobs;
using Beast.Claw.Reflection;
using Unity.Collections;

namespace Beast.Claw.Editor.Validation
{
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

                StaticDispose.Add(_dispose);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
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

            Native.PermHashSet(values.Length, out NativeHashSet<long> allValues);
            
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

        private static void _dispose()
        {
            foreach (EnumData data in ENUMS.Values)
            {
                if (!data.isFlags)
                {
                    data.allValues.Dispose();
                }
            }
            
            ENUMS.Clear();
        }
    }
}