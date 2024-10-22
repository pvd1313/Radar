using System;
using Unity.Mathematics;

namespace Beast.Radar.InputControl
{
    public interface IPlayerInputController
    {
        public event Action<float2> ClickMove;
    }
}