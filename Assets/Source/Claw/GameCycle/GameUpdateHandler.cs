using System;
using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public class GameUpdateHandler : Common.Mono
    {
        public event Action FrameBegin;
        public event Action FixedUpdated;
        public event Action FrameEnd;
        public event Action GizmosDraw;
        public event Action Destroyed;

        private void Update()
        {
            FrameBegin?.Invoke();
        }

        private void FixedUpdate()
        {
            FixedUpdated?.Invoke();
        }
        
        private void LateUpdate()
        {
            FrameEnd?.Invoke();
        }

        private void OnDrawGizmos()
        {
            GizmosDraw?.Invoke();
        }

        private void OnDestroy()
        {
            Destroyed?.Invoke();
        }
    }
}