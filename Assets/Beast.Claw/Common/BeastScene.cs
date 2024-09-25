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
    }
}