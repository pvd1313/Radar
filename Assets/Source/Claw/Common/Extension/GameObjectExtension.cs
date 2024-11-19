using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class GameObjectExtension
    {
        /// <returns>gameObject != null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlive(this GameObject gameObject)
        {
            return gameObject != null;
        }
    }
}