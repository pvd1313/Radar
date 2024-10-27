using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Resources
{
    public interface IResourceManager
    {
        UniTask<TObject> Load<TOwner, TObject>(TOwner owner, IAddress<TObject> address) 
            where TObject : Object
            where TOwner : class;

        void Release<TOwner, TObject>(TOwner owner, IAddress<TObject> address) 
            where TOwner : class
            where TObject : Object;
    }
}