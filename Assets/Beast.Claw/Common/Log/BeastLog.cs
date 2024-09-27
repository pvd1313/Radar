using System;
using Beast.Claw.Reflection;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Common
{
    [InitializeOnLoad]
    public static class BeastLog
    {
        public static BeastLogStateStorage StateStorage { get; private set; }

        static BeastLog()
        {
            try
            {
                StateStorage = new BeastLogStateStorage();
            }
            catch (Exception e)
            {
                InternalLogException(e);
            }
        }
        
        public static void Int<T>(T owner, string id, int value)
        {
            string detailedID = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(id);

            StateStorage.WriteInt(detailedID, value);
        }
        
        public static void Float<T>(T owner, string id, float value)
        {
            string detailedID = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(id);
            
            StateStorage.WriteFloat(detailedID, value);
        }

        public static void Float2<T>(T owner, string id, float2 value)
        {
            string detailedID = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(id);

            StateStorage.WriteFloat2(detailedID, value);
        }
        
        public static void Float3<T>(T owner, string id, float3 value)
        {
            string detailedID = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(id);

            StateStorage.WriteFloat3(detailedID, value);
        }
        
        public static void Float4<T>(T owner, string id, float4 value)
        {
            string detailedID = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(id);

            StateStorage.WriteFloat4(detailedID, value);
        }
        
        public static void String<T>(T owner, string id, string value)
        {
            string detailedID = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(id);

            StateStorage.WriteString(detailedID, value);
        }
        
        public static void Info<T>(T owner, string message)
        {
            InternalLogInfo(BeastTypeStorage<T>.FullName + ": " + message);
        }
        
        public static void Error<T>(T owner, string message)
        {
            InternalLogError(BeastTypeStorage<T>.FullName + ": " + message);
        }
        
        public static void StaticError(string message)
        {
            InternalLogError(message);
        }

        public static void Exception<T>(T owner, Exception e, string message)
        {
            InternalLogError(BeastTypeStorage<T>.FullName + ": " + message);
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