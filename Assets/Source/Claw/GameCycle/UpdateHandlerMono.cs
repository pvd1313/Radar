using System;
using Beast.Claw.Common;

namespace Beast.Claw.GameCycle
{
    public class UpdateHandlerMono : BeastMono
    {
        public event Action FrameBegin;
        public event Action FixedUpdated;
        public event Action FrameEnd;

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
    }
}