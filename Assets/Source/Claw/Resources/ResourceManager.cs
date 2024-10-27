using System;
using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Claw.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    public class ResourceManager : IResourceManager, IGameCycleListener
    {
        private class AssetInfo
        {
            public readonly HashSet<object> occupiedBy = new();
            public readonly Guid guid;
            public readonly AsyncOperationHandle handle;
            
            public Object asset;
            
            public AssetInfo(Guid guid, AsyncOperationHandle handle)
            {
                this.guid = guid;
                this.handle = handle;
            }
        }
        
        private Dictionary<Guid, AssetInfo> _assets;
        private HashSet<Guid> _activeLoads;
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _assets = new Dictionary<Guid, AssetInfo>();
            _activeLoads = new HashSet<Guid>();
            
            return ActionResult.SuccessSyncronous();
        }

        public async UniTask<TObject> Load<TOwner, TObject>(TOwner owner, IResource<TObject> reference) 
            where TObject : Object
            where TOwner : class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            reference.ValidateThrowException(nameof(reference));

            if (_assets.TryGetValue(reference.Guid, out AssetInfo info))
            {
                if (!info.occupiedBy.Add(owner))
                {
                    string message = $"'{owner.TypeName()}' tried to load asset twice: {reference.Guid}";

                    throw new InvalidOperationException(message);
                }

                if (_activeLoads.Contains(reference.Guid))
                {
                    await UniTask.WaitUntil(() => _activeLoads.Contains(reference.Guid)); 
                }

                return (TObject) _assets[reference.Guid].asset;
            }

            _activeLoads.Add(reference.Guid);
            
            AsyncOperationHandle handle = reference.Reference.LoadAssetAsync<TObject>();
            // handle.ReleaseHandleOnCompletion();
            
            info = new AssetInfo(reference.Guid, handle);
            info.occupiedBy.Add(owner);
            _assets[reference.Guid] = info;

            await UniTask.WaitUntil(() => handle.IsDone);
            
            info.asset = (Object) handle.Result;
            
            _activeLoads.Remove(info.guid);

            return (TObject) info.asset;
        }

        public void Release<TOwner, TObject>(TOwner owner, IResource<TObject> reference) 
            where TOwner : class
            where TObject : Object
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            reference.ValidateThrowException(nameof(reference));

            if (_activeLoads.Contains(reference.Guid))
            {
                string text =
                    $"'{owner.TypeName()}' tried to unload asset that is still being loaded: '{reference.Guid}'";
                throw new InvalidOperationException(text);
            }

            if (!_assets.TryGetValue(reference.Guid, out AssetInfo info))
            {
                throw new InvalidOperationException($"No asset loaded with guid: '{reference.Guid}'");
            }

            if (!info.occupiedBy.Remove(owner))
            {
                string text = $"Asset '{info.guid}' is not occupied by '{owner.TypeName()}'";
                throw new InvalidOperationException(text);
            }

            if (info.occupiedBy.Count == 0)
            {
                Addressables.Release(info.handle);

                _assets.Remove(info.guid);
            }
        }
    }
}