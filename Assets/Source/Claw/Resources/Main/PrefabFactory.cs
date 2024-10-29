using System;
using Beast.Claw.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Beast.Claw.Resources
{
    public class PrefabFactory : IPrefabFactory
    {
        public async UniTask<TMono> Instantiate<TMono>(IController owner, TMono prefab) where TMono : MonoBehaviour
        {
            prefab.ValidateNotNullThrowException(nameof(prefab));

            AsyncInstantiateOperation<TMono> operation = Object.InstantiateAsync(prefab);

            await UniTask.WaitUntil(() => operation.isDone);
            
            if (operation.Result.Length == 0 || operation.Result[0] == null)
            {
                throw new Exception($"Failed to instantiate prefab ({prefab.name})");
            }

            TMono result = operation.Result[0];

            return result;
        }
    }
}