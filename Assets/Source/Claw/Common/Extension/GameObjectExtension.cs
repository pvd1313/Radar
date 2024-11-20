using System.Runtime.CompilerServices;
using Beast.Claw.Attributes.Annotation;
using UnityEngine;

namespace Beast.Claw.Common
{
    [ExtensionClass]
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