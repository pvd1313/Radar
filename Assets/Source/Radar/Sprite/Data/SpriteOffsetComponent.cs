using Beast.Claw.Math;
using Unity.Entities;

namespace Beast.Radar.Sprite
{
    public struct SpriteOffsetComponent : IComponentData
    {
        public sbyte2 value;

        public SpriteOffsetComponent(sbyte2 value)
        {
            this.value = value;
        }
    }
}