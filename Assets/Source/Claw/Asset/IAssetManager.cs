using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Asset
{
    public interface IAssetManager
    {
        UniTask<TObject> Load<TOwner, TObject>(TOwner owner, IAssetReference<TObject> reference) 
            where TObject : Object
            where TOwner : class;

        void Dispose<TOwner, TObject>(TOwner owner, IAssetReference<TObject> reference) 
            where TOwner : class
            where TObject : Object;
    }
}