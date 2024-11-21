using System.Diagnostics;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Common
{
    [StaticUtility]
    public static class BScene
    {
        public static GameObject CreateGameObject<TOwner>(TOwner owner)
        {
            GameObject gameObject = new (TypeStorage<TOwner>.FULL_NAME);

            return gameObject;
        }

        /// <summary>
        /// Instantiates MonoBehaviour prefab at zero position with identity rotation.
        /// </summary>
        public static TMono InstantiateMono<TMono>(IController owner, TMono prefab) 
            where TMono : MonoBehaviour
        {
            return InstantiateMono(owner, prefab, float3.zero, quaternion.identity);
        }
        
        public static TMono InstantiateMono<TMono>(IController owner, TMono prefab, float3 position, quaternion rotation) 
            where TMono : MonoBehaviour
        {
            TMono mono = Object.Instantiate(prefab, position, rotation);

            _attachDebugInfo(mono.gameObject, owner);
            
            return mono;
        }

        [Conditional("UNITY_EDITOR")]
        private static void _attachDebugInfo(GameObject toObject, IController owner)
        {
            MonoDebugInfo info = toObject.AddComponent<MonoDebugInfo>();
            
            info.owner = owner.Meta;
        }
    }
}