using UnityEngine;

namespace RisenBeast.Common
{
    public static class SceneUtil
    {
        public static GameObject NewObjectFor<T>()
        {
            GameObject gameObject = new GameObject(typeof(T).FullName);

            return gameObject;
        }
    }
}