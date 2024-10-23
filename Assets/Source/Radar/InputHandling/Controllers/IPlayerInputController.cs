using System;
using Unity.Mathematics;

namespace Beast.Radar.InputHandling
{
    public interface IPlayerInputController
    {
        public event Action<float2> ClickMove;
    }
}