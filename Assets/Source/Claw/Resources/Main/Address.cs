using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    [Serializable]
    public class Address<TObject> : ISerializationCallbackReceiver, IAddress<TObject> 
        where TObject : Object
    {
        public Type InnerType { get; } = typeof(TObject);

        public Guid Guid => _guid;
        
        public AssetReference Reference => _reference;
        
        [SerializeField] private AssetReference _reference;

        private Guid _guid;
        private bool _hasGuid;
        
        public string Validate()
        {
            if (!_hasGuid)
            {
                return "Reference is not set or GUID parsing failed.";
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