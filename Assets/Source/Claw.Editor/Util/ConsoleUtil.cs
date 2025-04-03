using System;
using System.Reflection;

namespace Beast.Claw.Editor
{
    public static class ConsoleUtil
    {
        private static readonly MethodInfo ClearConsoleMethod;
        private static readonly bool IsClearConsoleMethodFound;
        
        static ConsoleUtil()
        {
            try
            {
                var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
                var type = assembly.GetType("UnityEditor.LogEntries");
                ClearConsoleMethod = type?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
                IsClearConsoleMethodFound = !(ClearConsoleMethod is null);
            }
            catch (Exception e)
            {
                IsClearConsoleMethodFound = false;
                
                UnityEngine.Debug.LogException(e);
            }
        }

        public static void ClearLog()
        {
            if (!IsClearConsoleMethodFound)
            {
                UnityEngine.Debug.LogError("Clear console method was not found");
                return;
            }

            ClearConsoleMethod.Invoke(null, null);
        }
    }
}