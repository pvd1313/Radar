using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Beast.Claw.Collection;
using Beast.Claw.Common;
using Beast.Claw.Debug;

using Object = UnityEngine.Object;

namespace Beast.Claw.Reflection
{
    public static class Reflect
    {
        private static readonly ConcurrentDictionary<Type, AttributeData[]> ATTRIBUTES;
        private static readonly RingPool<List<AttributeData>> ATTRIBUTE_POOL;

        static Reflect()
        {
            try
            {
                ATTRIBUTES = new ConcurrentDictionary<Type, AttributeData[]>();
                ATTRIBUTE_POOL = new RingPool<List<AttributeData>>(16, () => new List<AttributeData>());
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
        /// <summary>
        /// Cashed version of System.Type.GetCustomAttributes(). inherit = true.
        /// </summary>
        public static IReadOnlyList<AttributeData> GetCustomAttributes(Type type)
        {
            return ATTRIBUTES.GetOrFactory(type, _getAttributes);
        }
        
        /// <exception cref="InvalidOperationException"></exception>
        public static long UnboxAsLong(object value, TypeCode code)
        {
            return code switch
            {
                TypeCode.SByte => (sbyte)value,
                TypeCode.Byte => (byte) value,
                TypeCode.Int16 => (short)value,
                TypeCode.UInt16 => (ushort)value,
                TypeCode.Int32 => (int)value,
                TypeCode.UInt32 => (uint)value,
                TypeCode.Int64 => (long)value,
                TypeCode.UInt64 => (long)(ulong)value,
                _ => throw new InvalidOperationException($"Invalid underlying type: {code}")
            };
        }

        private static AttributeData[] _getAttributes(Type type)
        {
            List<AttributeData> list = ATTRIBUTE_POOL.Next();

            foreach (Attribute attribute in type.GetCustomAttributes())
            {
                list.Add(new AttributeData
                {
                    attribute = attribute,
                    type = attribute.GetType()
                });
            }

            return list.ToArray();
        }
        
        public static void GetField(Type owner, string fieldName, BindingFlags flags, ref FieldInfo cash)
        {
            if (cash != null)
            {
                return;
            }

            FieldInfo field = owner.GetField(fieldName, flags);

            if (field == null)
            {
                throw new Exception($"Failed to get field {owner} ({fieldName})");
            }

            cash = field;
        }

#if UNITY_EDITOR
        /// <summary>Editor only. Will not open window, it must be created to be found.</summary>
        /// <remarks>Uses UnityEngine.Resources.FindObjectsOfTypeAll()</remarks>
        /// <exception cref="Exception"></exception>
        public static void FindInternalUnityEditorWindow(Type windowType, ref UnityEditor.EditorWindow cash)
        {
            if (!TryFindInternalUnityEditorWindow(windowType, ref cash))
            {
                throw new Exception($"Failed to find window: {windowType.FullName}");
            };
        }
        
        /// <summary>Editor only. Will not open window, it must be created to be found.</summary>
        /// <remarks>Uses UnityEngine.Resources.FindObjectsOfTypeAll()</remarks>
        /// <exception cref="Exception"></exception>
        public static bool TryFindInternalUnityEditorWindow(Type windowType, ref UnityEditor.EditorWindow cash)
        {
            BDebug.SignalPerformanceCriticalContext();
            
            if (cash != null)
            {
                return true;
            }

            Object[] windows = UnityEngine.Resources.FindObjectsOfTypeAll(windowType);

            if (windows.Length == 0)
            {
                return false;
            }

            if (windows.Length > 1)
            {
                throw new Exception($"Found too many windows ({windows.Length}): {windowType.FullName}");
            }

            if (windows[0] is not UnityEditor.EditorWindow targetWindow)
            {
                throw new Exception(
                    $"Failed to cast window ({windows[0].GetType().FullName} -> UnityEditor.EditorWindow)");
            }

            cash = targetWindow;

            return true;
        }
#endif
        
        public static void TypeFromAssembly<TAssembly>(string name, ref Type cash)
        {
            if (cash != null)
            {
                return;
            }

            cash = typeof(TAssembly).Assembly.GetType(name, true);
        }
    }
}