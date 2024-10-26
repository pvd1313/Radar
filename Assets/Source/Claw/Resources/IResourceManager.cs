using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Resources
{
    public interface IResourceManager
    {
        UniTask<TObject> Load<TOwner, TObject>(TOwner owner, IResource<TObject> reference) 
            where TObject : Object
            where TOwner : class;

        void Release<TOwner, TObject>(TOwner owner, IResource<TObject> reference) 
            where TOwner : class
            where TObject : Object;
    }
}