using System;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BeastLog
    {
        public static void Error<T>(T owner, string message)
        {
            Error(typeof(T).FullName + ": " + message);
        }

        public static void Exception<T>(T owner, Exception e, string message)
        {
            Error(owner, message);
            
            Debug.LogException(e);
        }
        
        private static void Error(string message)
        {
            Debug.LogError(message);
        }
    }
}