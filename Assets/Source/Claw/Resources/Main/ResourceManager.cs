using System;
using System.Collections.Generic;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Reflection;
using Beast.Claw.Text;
using Beast.Claw.Validation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    public class ResourceManager : Actor, IResourceManager
    {
        private struct GameObjectReference
        {
            public AssetReference reference;
            public GameObject gameObject;
        }
        
        private List<AssetReference> _loadedAssets;
        private List<GameObjectReference> _loadedGameObjects;
        private RingPool<GameObjectAddressAdapter> _adapterPool;
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            _loadedAssets = new List<AssetReference>(256);
            _loadedGameObjects = new List<GameObjectReference>(64);
            _adapterPool = new RingPool<GameObjectAddressAdapter>(64, () => new GameObjectAddressAdapter());

            return UniTask.CompletedTask;
        }

        public async UniTask<TObject> LoadAsset<TObject>(IController owner, IAddress<TObject> address)
            where TObject : Object
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            address.ValidateThrowException(nameof(address));

            if (Build.IS_DEVELOPMENT)
            {
                Type objType = typeof(TObject);

                if (objType.Implements(UnityType.GAME_OBJECT) || objType.Implements(UnityType.MONO_BEHAVIOUR))
                {
                    throw new InvalidOperationException(
                        "ResourceManager: LoadAsset() can not be called for " +
                        "GameObject or MonoBehaviour. Use Instantiate() instead.");
                }
            }

            AsyncOperationHandle operationHandle = address.Reference.OperationHandle;

            if (operationHandle.IsValid())
            {
                if (operationHandle.Status == AsyncOperationStatus.None)
                {
                    await operationHandle.Task;

                    if (operationHandle.Status == AsyncOperationStatus.Failed)
                    {
                        throw new InvalidOperationException($"ResourceManager: Failed to load asset: {address.Guid.Q()}");
                    }
                }

                return (TObject)address.Reference.OperationHandle.Result;
            }

            AsyncOperationHandle<TObject> handle = address.Reference.LoadAssetAsync<TObject>();

            await handle.Task;

            if (address.Reference.OperationHandle.Status == AsyncOperationStatus.Failed)
            {
                throw new InvalidOperationException($"ResourceManager: Failed to load asset: {address.Guid.Q()}");
            }

            _loadedAssets.Add(address.Reference);

            return handle.Result;
        }

        public async UniTask<TMono> InstantiateMono<TMono>(IController owner, IAddress<TMono> address)
            where TMono : MonoBehaviour
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            address.ValidateThrowException(nameof(address));

            GameObjectAddressAdapter adapter = _adapterPool.Next();

            adapter.Wrap(address);
            
            GameObject gameObject = await LoadAsset(owner, adapter);

            TMono mono = gameObject.GetComponent<TMono>();

            if (mono == null)
            {
                StringBuilder str = new(256);
                str = str + "ResourceManager: MonoBehaviour '" + typeof(TMono).FullName + "' was not found in ";
                str = str + "GameObject '" + address.Guid.ToString() + "(" + gameObject.name + ")";

                throw new Exception(str.ToString());
            }

            return mono;
        }

        public async UniTask<GameObject> InstantiateGameObject(IController owner, IAddress<GameObject> address)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            address.ValidateThrowException(nameof(address));

            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address.Reference);
            
            await handle.Task;

            GameObject gameObject = handle.Result;

            if (gameObject == null)
            {
                throw new Exception($"ResourceManager: Failed to load GameObject from asset {address.Guid.Q()}");
            }

            _loadedGameObjects.Add(new GameObjectReference
            {
                gameObject = gameObject, 
                reference = address.Reference
            });

            return gameObject;
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
            
            foreach (GameObjectReference reference in _loadedGameObjects)
            {
                if (reference.gameObject.IsAlive() && reference.reference.IsValid())
                {
                    reference.reference.ReleaseInstance(reference.gameObject);
                }
            }
            
            _loadedAssets.Clear();
            _loadedGameObjects.Clear();
        }
    }
}