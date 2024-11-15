using System;
using System.Reflection;
using Beast.Claw.Debug;
using Object = UnityEngine.Object;

namespace Beast.Claw.Reflection
{
    public static class ReflectionCash
    {
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
                throw new Exception($"Found too much windows ({windows.Length}): {windowType.FullName}");
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