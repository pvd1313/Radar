using Beast.Claw.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BeastScene
    {
        public static GameObject NewGameObject<TOwner>(TOwner owner)
        {
            GameObject gameObject = new (BeastTypeStorage<TOwner>.FullName);

            return gameObject;
        }
        
        public static TMono InstantiateMono<TOwner, TMono>
            (TOwner owner, TMono prefab, float3 position, quaternion rotation) where TMono : BeastMono
        {
            TMono mono = Object.Instantiate(prefab, position, rotation);

            mono.gameObject.name = BeastTypeStorage<TOwner>.FullNameWithPostfixOrFullName(prefab.name);
            
            return mono;
        }

        public static GameObject InstantiateGameObject<TOwner>
            (TOwner owner, GameObject prefab, float3 position, quaternion rotation)
        {
            GameObject gameObject = Object.Instantiate(prefab, position, rotation);

            gameObject.name = BeastTypeStorage<TOwner>.FullNameWithPostfixOrFullName(prefab.name);

            return gameObject;
        }
    }
}