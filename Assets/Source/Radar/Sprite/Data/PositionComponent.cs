using Unity.Entities;
using Unity.Mathematics;

namespace Beast.Radar.Sprite
{
    public struct PositionComponent : IComponentData
    {
        public float2 value;

        public PositionComponent(float2 value)
        {
            this.value = value;
        }
    }
}