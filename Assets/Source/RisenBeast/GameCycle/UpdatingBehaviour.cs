using System;
using RisenBeast.Common;
using UnityEngine;

namespace RisenBeast.GameCycle
{
    public class UpdatingBehaviour : BeastMonoBehaviour
    {
        public event Action Updated;
        public event Action LateUpdated;
        public event Action FixedUpdated;

        private void Update()
        {
            Updated?.Invoke();
        }

        private void LateUpdate()
        {
            LateUpdated?.Invoke();
        }

        private void FixedUpdate()
        {
            FixedUpdated?.Invoke();
        }
    }
}