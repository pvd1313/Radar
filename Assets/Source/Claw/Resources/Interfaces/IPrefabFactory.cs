using Beast.Claw.Common;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Resources
{
    public interface IPrefabFactory
    {
        UniTask<TMono> Instantiate<TMono>(IController owner, TMono prefab) where TMono : MonoBehaviour;
    }
}