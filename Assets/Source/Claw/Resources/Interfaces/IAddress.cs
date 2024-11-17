using System;
using Beast.Claw.Common;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    public interface IAddress<TObject> : IValidatable where TObject : Object
    {
        public Guid Guid { get; }
        
        public AssetReference Reference { get;}
    }
}