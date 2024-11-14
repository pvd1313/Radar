using System;
using System.Reflection;

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
        public static void GetUnityEditorWindow(Type windowType, ref UnityEditor.EditorWindow cash)
        {
            if (cash != null)
            {
                return;
            }

            UnityEditor.EditorWindow window = UnityEditor.EditorWindow.GetWindow(windowType);
            
            if (window == null)
            {
                throw new Exception($"Failed: EditorWindow.GetWindow({windowType.FullName})");
            }

            cash = window;
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