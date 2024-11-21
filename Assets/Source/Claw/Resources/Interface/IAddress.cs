using System;
using Beast.Claw.Validation;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    public interface IAddress<TObject> : IValidatable where TObject : Object
    {
        public Type InnerType { get; }
        
        public Guid Guid { get; }
        
        public AssetReference Reference { get;}
    }
}