using System.Collections;
using RisenBeast.Common;
using UnityEngine;

namespace RisenBeast.Coroutines
{
    public class CoroutineController
    {
        private MonoBehaviour _behaviour;
        
        public void Initialize()
        {
            _behaviour = SceneUtil.NewObjectFor<CoroutineController>().AddComponent<RisenBeastMonoBehaviour>();
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