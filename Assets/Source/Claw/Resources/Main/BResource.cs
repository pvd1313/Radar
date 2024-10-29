using Beast.Claw.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    public class BResource
    {
        public static async UniTask<TObject> LoadAsset<TObject>(IController owner, IAddress<TObject> address) where TObject : Object
        {
            if (address.Reference.OperationHandle.IsValid())
            {
                if (address.Reference.OperationHandle.Status == AsyncOperationStatus.None)
                {
                    await address.Reference.OperationHandle.Task;
                }
                
                return (TObject)address.Reference.OperationHandle.Result;
            }

            AsyncOperationHandle<TObject> handle = address.Reference.LoadAssetAsync<TObject>();

            await handle.Task;

            return handle.Result;
        }

        public static async UniTask<TMono> LoadPrefab<TMono>(IController owner, IAddress<TMono> address) where TMono : MonoBehaviour
        {
            if (address.Reference.OperationHandle.IsValid())
            {
                if (address.Reference.OperationHandle.Status == AsyncOperationStatus.None)
                {
                    await address.Reference.OperationHandle.Task;
                }
                
                return ((GameObject)address.Reference.OperationHandle.Result).GetComponent<TMono>();
            }

            AsyncOperationHandle<GameObject> handle = address.Reference.LoadAssetAsync<GameObject>();

            await handle.Task;

            return handle.Task.Result.GetComponent<TMono>();
        }
    }
}