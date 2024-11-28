using UnityEngine;
using VContainer.Unity;

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

                LifetimeScope scope = Object.FindObjectOfType<LifetimeScope>();
                
                if (scope != null)
                {
                    scope.Container.TryResolve(typeof(TObject), out object resolved);

                    _reference = resolved as TObject;
                }
            }

            return _reference;
        }
    }
}