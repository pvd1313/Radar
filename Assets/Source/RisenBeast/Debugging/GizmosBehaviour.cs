using System;
using UnityEngine;

namespace RisenBeast.Debugging
{
    public class GizmosBehaviour : MonoBehaviour
    {
        public event Action GizmosDrawn;
        
        private void OnDrawGizmos()
        {
            GizmosDrawn?.Invoke();
        }
    }
}