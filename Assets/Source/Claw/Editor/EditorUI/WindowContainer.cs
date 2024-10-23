using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    [System.Serializable]
    public class WindowContainer<TWindow> where TWindow : class
    {
        public TWindow Window => _reference as TWindow;
        
        [SerializeField] private Object _reference;
        
        public void SetReference(Object reference)
        {
            _reference = reference;
        }
    }
}