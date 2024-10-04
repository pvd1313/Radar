using System.Collections;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Coroutines
{
    public class BeastCoroutineController : IBeastInitialize
    {
        private MonoBehaviour _behaviour;
        
        public UniTask<BeastInitializationResult> Initialize()
        {
            _behaviour = BeastScene.NewGameObject(this).AddComponent<BeastMono>();
            
            return BeastInitializationResult.SuccessNotAsync();
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            return _behaviour.StartCoroutine(routine);
        }

        public void StopCoroutine(Coroutine routine)
        {
            if (routine == null)
            {
                throw new BeastException("Can`t stop a null coroutine.");
            }
            _behaviour.StopCoroutine(routine);
        }
    }
}