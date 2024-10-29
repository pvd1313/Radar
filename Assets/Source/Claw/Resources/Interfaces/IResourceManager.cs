using Beast.Claw.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Resources
{
    public interface IResourceManager
    {
        UniTask<TObject> LoadAsset<TObject>(IController owner, IAddress<TObject> address)
            where TObject : Object;

        UniTask<TMono> LoadMono<TMono>(IController owner, IAddress<TMono> address)
            where TMono : MonoBehaviour;

        UniTask<GameObject> LoadGameObject(IController owner, IAddress<GameObject> address);
    }
}