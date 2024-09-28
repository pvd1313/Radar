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
        
        public static T Instantiate<T>(T prefab, float3 position, quaternion rotation ) where T : BeastMono
        {
            T mono = Object.Instantiate(prefab, position, rotation);

            mono.gameObject.name = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(prefab.name);
            
            return mono;
        }
    }
}