using Beast.Claw.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BeastScene
    {
        public static GameObject NewGameObject<T>(T owner)
        {
            GameObject gameObject = new (BeastTypeStorage<T>.FullName);

            return gameObject;
        }
        
        public static T NewInstance<T>(T prefab, float3 position, quaternion rotation) where T : BeastMono
        {
            return Object.Instantiate(prefab, position, rotation);
        }
    }
}