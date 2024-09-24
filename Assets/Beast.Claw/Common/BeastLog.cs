using System;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BeastLog
    {
        public static void Info<T>(T owner, string message)
        {
            InternalLogInfo(typeof(T).FullName + ": " + message);
        }
        
        public static void Error<T>(T owner, string message)
        {
            InternalLogError(typeof(T).FullName + ": " + message);
        }
        
        public static void StaticError(string message)
        {
            InternalLogError(message);
        }

        public static void Exception<T>(T owner, Exception e, string message)
        {
            InternalLogError(typeof(T).FullName + ": " + message);
            InternalLogException(e);
        }

        public static void StaticException(Exception e, string message)
        {
            InternalLogError(message);
            InternalLogException(e);
        }

        private static void InternalLogInfo(string message)
        {
            Debug.Log(message);
        }
        
        private static void InternalLogError(string message)
        {
            Debug.LogError(message);
        }
        private static void InternalLogException(Exception e)
        {
            Debug.LogException(e);
        }
    }
}