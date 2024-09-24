using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BeastScene
    {
        public static GameObject NewObjectFor<T>()
        {
            GameObject gameObject = new GameObject(typeof(T).FullName);

            return gameObject;
        }
    }
}