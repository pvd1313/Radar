using System;
using Beast.Claw.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    public class BFactory
    {
        public static async UniTask<TMono> InstantiateMono<TMono>(IController owner, TMono prefab) where TMono : MonoBehaviour
        {
            prefab.ValidateNotNullThrowException(nameof(prefab));

            AsyncInstantiateOperation<TMono> operation = Object.InstantiateAsync(prefab);

            await UniTask.WaitUntil(() => operation.isDone);
            
            if (operation.Result.Length == 0 || operation.Result[0] == null)
            {
                throw new Exception($"Failed to instantiate prefab ({prefab.name})");
            }

            return operation.Result[0];
        }
        
        public static async UniTask<GameObject> InstantiateGameObject(IController owner, GameObject prefab)
        {
            prefab.ValidateNotNullThrowException(nameof(prefab));

            AsyncInstantiateOperation<GameObject> operation = Object.InstantiateAsync(prefab);

            await UniTask.WaitUntil(() => operation.isDone);
            
            if (operation.Result.Length == 0 || operation.Result[0] == null)
            {
                throw new Exception($"Failed to instantiate prefab ({prefab.name})");
            }

            return operation.Result[0];
        }
    }
}