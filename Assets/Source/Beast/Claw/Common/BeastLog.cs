using System;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BeastLog
    {
        public static void Error(string message)
        {
            Debug.LogError(message);
        }

        public static void ErrorFor<T>(string message)
        {
            Error(typeof(T).FullName + ": " + message);
        }

        public static void ExceptionFor<T>(Exception e, string message)
        {
            ErrorFor<T>(message);
            
            Debug.LogException(e);
        }
    }
}