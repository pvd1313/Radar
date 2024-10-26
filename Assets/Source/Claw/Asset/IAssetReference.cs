using System;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Beast.Claw.Asset
{
    public interface IAssetReference<TObject> : IValidatable where TObject : Object
    {
        public Guid Guid { get; }
        
        public AssetReferenceT<TObject> Reference { get;}
    }
}