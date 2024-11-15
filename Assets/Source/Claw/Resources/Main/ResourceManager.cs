using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    public class ResourceManager : Actor, IResourceManager
    {
        private List<AssetReference> _loadedAssets;

        public async UniTask<TObject> LoadAsset<TObject>(IController owner, IAddress<TObject> address)
            where TObject : Object
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            address.ValidateThrowException(nameof(address));

            if (address.Reference.OperationHandle.IsValid())
            {
                if (address.Reference.OperationHandle.Status == AsyncOperationStatus.None)
                {
                    await address.Reference.OperationHandle.Task;

                    if (address.Reference.OperationHandle.Status == AsyncOperationStatus.Failed)
                    {
                        throw new InvalidOperationException($"Failed to load asset ({address.Guid}");
                    }
                }

                return (TObject)address.Reference.OperationHandle.Result;
            }

            AsyncOperationHandle<TObject> handle = address.Reference.LoadAssetAsync<TObject>();

            await handle.Task;

            if (address.Reference.OperationHandle.Status == AsyncOperationStatus.Failed)
            {
                throw new InvalidOperationException($"Failed to load asset ({address.Guid}");
            }

            _loadedAssets.Add(address.Reference);

            return handle.Result;
        }

        public async UniTask<TMono> LoadMono<TMono>(IController owner, IAddress<TMono> address)
            where TMono : MonoBehaviour
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            address.ValidateThrowException(nameof(address));

            if (address.Reference.OperationHandle.IsValid())
            {
                if (address.Reference.OperationHandle.Status == AsyncOperationStatus.None)
                {
                    await address.Reference.OperationHandle.Task;

                    if (address.Reference.OperationHandle.Status == AsyncOperationStatus.Failed)
                    {
                        throw new InvalidOperationException($"Failed to load mono ({address.Guid}");
                    }
                }

                return ((GameObject)address.Reference.OperationHandle.Result).GetComponent<TMono>();
            }

            AsyncOperationHandle<GameObject> handle = address.Reference.LoadAssetAsync<GameObject>();

            await handle.Task;

            if (address.Reference.OperationHandle.Status == AsyncOperationStatus.Failed)
            {
                throw new InvalidOperationException($"Failed to load mono ({address.Guid}");
            }

            _loadedAssets.Add(address.Reference);

            return handle.Task.Result.GetComponent<TMono>();
        }

        public async UniTask<GameObject> LoadGameObject(IController owner, IAddress<GameObject> address)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            address.ValidateThrowException(nameof(address));

            if (address.Reference.OperationHandle.IsValid())
            {
                if (address.Reference.OperationHandle.Status == AsyncOperationStatus.None)
                {
                    await address.Reference.OperationHandle.Task;

                    if (address.Reference.OperationHandle.Status == AsyncOperationStatus.Failed)
                    {
                        throw new InvalidOperationException($"Failed to load GameObject ({address.Guid}");
                    }
                }

                return (GameObject)address.Reference.OperationHandle.Result;
            }

            AsyncOperationHandle<GameObject> handle = address.Reference.LoadAssetAsync<GameObject>();

            await handle.Task;

            if (address.Reference.OperationHandle.Status == AsyncOperationStatus.Failed)
            {
                throw new InvalidOperationException($"Failed to load GameObject ({address.Guid}");
            }

            _loadedAssets.Add(address.Reference);

            return handle.Task.Result;
        }

        protected override UniTask _warmup(ConcurrentContext context)
        {
            _loadedAssets = new List<AssetReference>(256);

            return UniTask.CompletedTask;
        }

        protected override void _dispose(BlockingContext context)
        {
            // Technically this is required only for Editor.
            // Without it some assets may can junked upon load (DebugLogConsole).
            foreach (AssetReference assetReference in _loadedAssets)
            {
                if (assetReference.IsValid())
                {
                    assetReference.ReleaseAsset();
                }
            }
            
            _loadedAssets.Clear();
        }
    }
}