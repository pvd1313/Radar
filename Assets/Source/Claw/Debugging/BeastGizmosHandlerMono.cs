using System;
using UnityEngine;

namespace Beast.Claw.Debugging
{
    public class BeastGizmosHandlerMono : MonoBehaviour
    {
        public event Action GizmosDrawn;
        
        private void OnDrawGizmos()
        {
            GizmosDrawn?.Invoke();
        }
    }
}