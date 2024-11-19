using Beast.Claw.Attributes;
using UnityEngine;

namespace Beast.Claw.Common
{
    [FancyAsset]
    public abstract class Scriptable : ScriptableObject
    {
        public int SerializationVersion { get; private set; }

        private void OnValidate()
        {
            SerializationVersion++;
        }
    }
}