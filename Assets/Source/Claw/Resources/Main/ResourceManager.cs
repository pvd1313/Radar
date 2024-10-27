using System;
using System.Collections.Generic;
using System.Text;
using Beast.Claw.Debug;
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
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _assets = new Dictionary<Guid, AssetInfo>();
            _activeLoads = new HashSet<Guid>();
            
            return ActionResult.SuccessSyncronous();
        }

        public async UniTask<TObject> Load<TOwner, TObject>(TOwner owner, IAddress<TObject> address) 
            where TObject : Object
            where TOwner : class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            address.ValidateThrowException(nameof(address));

            if (_assets.TryGetValue(address.Guid, out AssetInfo info))
            {
                if (!info.occupiedBy.Add(owner))
                {
                    string message = $"'{owner.TypeName()}' tried to load asset twice: {address.Guid}";

                    throw new InvalidOperationException(message);
                }

                if (_activeLoads.Contains(address.Guid))
                {
                    await UniTask.WaitUntil(() => _activeLoads.Contains(address.Guid)); 
                }

                return (TObject) _assets[address.Guid].asset;
            }

            _activeLoads.Add(address.Guid);
            
            AsyncOperationHandle handle = address.Reference.LoadAssetAsync<TObject>();
            
            info = new AssetInfo(address.Guid, handle);
            info.occupiedBy.Add(owner);
            _assets[address.Guid] = info;

            await UniTask.WaitUntil(() => handle.IsDone);
            
            info.asset = (Object) handle.Result;
            
            _activeLoads.Remove(info.guid);

            return (TObject) info.asset;
        }

        public void Release<TOwner, TObject>(TOwner owner, IAddress<TObject> address) 
            where TOwner : class
            where TObject : Object
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            address.ValidateThrowException(nameof(address));

            if (_activeLoads.Contains(address.Guid))
            {
                string text =
                    $"'{owner.TypeName()}' tried to unload asset that is still being loaded: '{address.Guid}'";
                throw new InvalidOperationException(text);
            }

            if (!_assets.TryGetValue(address.Guid, out AssetInfo info))
            {
                throw new InvalidOperationException($"No asset loaded with guid: '{address.Guid}'");
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
        
        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            if (_assets.Count > 0)
            {
                StringBuilder builder = new ();
                builder.AppendLine("Some assets are not being released upon dispose:");

                foreach ((Guid guid, AssetInfo info) in _assets)
                {
                    builder.Append(' ', 4);
                    builder.Append(guid);

                    if (!_activeLoads.Contains(guid))
                    {
                        builder.Append('(');
                        builder.Append(info.asset.name);
                        builder.Append(')');
                    }
                    
                    builder.Append(": ");

                    int i = 0;
                    foreach (object owner in info.occupiedBy)
                    {
                        builder.Append(owner.GetType().FullName);

                        if (i < info.occupiedBy.Count - 1)
                        {
                            builder.Append(", ");
                        }
                        
                        i++;
                    }

                    builder.AppendLine();
                    
                    Addressables.Release(info.handle);
                }
                
                _assets.Clear();
                _activeLoads.Clear();
                
                BDebug.LogError(this, builder.ToString());
            }
            
            return ActionResult.SuccessSyncronous();
        }
    }
}