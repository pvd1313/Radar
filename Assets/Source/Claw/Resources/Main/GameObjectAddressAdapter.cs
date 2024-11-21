using System;
using Beast.Claw.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Beast.Claw.Resources
{
    public class GameObjectAddressAdapter : IAddress<GameObject>
    {
        public Type InnerType { get; } = UnityType.GAME_OBJECT;
        public Guid Guid { get; private set; }
        public AssetReference Reference { get; private set; }
        
        public void Wrap<TMono>(IAddress<TMono> monoAddress) where TMono : MonoBehaviour
        {
            Guid = monoAddress.Guid;
            Reference = monoAddress.Reference;
        }
        
        public string Validate()
        {
            return null;
        }
    }
}