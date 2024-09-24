using UnityEngine;

namespace Beast.Claw.Common
{
    public static class BeastScene
    {
        public static GameObject NewGameObject<T>(T owner)
        {
            GameObject gameObject = new GameObject(typeof(T).FullName);

            return gameObject;
        }
    }
}