using Beast.Claw.Reflection;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BScene
    {
        public static GameObject CreateGameObject<TOwner>(TOwner owner)
        {
            GameObject gameObject = new (TypeStorage<TOwner>.FULL_NAME);

            return gameObject;
        }
        
        public static TMono InstantiateMono<TOwner, TMono>
            (TOwner owner, TMono prefab, float3 position, quaternion rotation) where TMono : MonoBehaviour
        {
            TMono mono = Object.Instantiate(prefab, position, rotation);

            mono.gameObject.name = TypeStorage<TOwner>.FullNameWithPrefixOrFullName(prefab.name);
            
            return mono;
        }

        public static GameObject InstantiateGameObject<TOwner>
            (TOwner owner, GameObject prefab, float3 position, quaternion rotation)
        {
            GameObject gameObject = Object.Instantiate(prefab, position, rotation);

            gameObject.name = TypeStorage<TOwner>.FullNameWithPrefixOrFullName(prefab.name);

            return gameObject;
        }
    }
}