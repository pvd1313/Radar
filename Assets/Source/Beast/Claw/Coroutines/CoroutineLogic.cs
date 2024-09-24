using System.Collections;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Coroutines
{
    public class CoroutineLogic : Initializable
    {
        private MonoBehaviour _behaviour;
        
        public UniTask<InitializationResult> Initialize()
        {
            _behaviour = BeastScene.NewObjectFor<CoroutineLogic>().AddComponent<BeastMono>();
            
            return InitializationResult.SuccessNotAsync();
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return _behaviour.StartCoroutine(routine);
        }

        public void StopCoroutine(Coroutine routine)
        {
            _behaviour.StopCoroutine(routine);
        }
    }
}