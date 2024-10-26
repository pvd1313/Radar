using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Beast.Claw.Asset
{
    [Serializable]
    public class AssetReference<TObject> : ISerializationCallbackReceiver, IAssetReference<TObject> 
        where TObject : Object
    {
        public Guid Guid => _guid;
        
        public AssetReferenceT<TObject> Reference => _reference;
        
        [SerializeField] private AssetReferenceT<TObject> _reference;

        private Guid _guid;
        private bool _hasGuid;
        
        public string Validate()
        {
            if (!_hasGuid)
            {
                return "Reference not set or GUID parsing failed.";
            }
            
            return null;
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            _guid = Guid.Empty;
            
            _hasGuid = _reference != null && 
                       !string.IsNullOrEmpty(_reference.AssetGUID) && 
                       Guid.TryParse(_reference.AssetGUID, out _guid);
        }
    }
}