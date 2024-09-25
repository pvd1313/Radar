using Beast.Claw.Reflection;
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
        
        public static T NewInstance<T>(T prefab, Vector3 position, Quaternion rotation) where T : BeastMono
        {
            return Object.Instantiate(prefab, position, rotation);
        }
    }
}