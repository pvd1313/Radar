using UnityEngine;
using Zenject;

namespace Beast.Claw.Editor
{
    public class InjectionReference<TObject> where TObject : class
    {
        public TObject Reference => _tryResolve();

        private bool _referenceSearched;
        private TObject _reference;

        private TObject _tryResolve()
        {
            if (!Application.isPlaying)
            {
                _referenceSearched = false;
                _reference = null;
                
                return null;
            }

            if (!_referenceSearched)
            {
                _referenceSearched = true;
                _reference = null;

                SceneContext context = Object.FindObjectOfType<SceneContext>();
                if (context != null)
                {
                    _reference = context.Container.TryResolve<TObject>();
                }
            }

            return _reference;
        }
    }
}