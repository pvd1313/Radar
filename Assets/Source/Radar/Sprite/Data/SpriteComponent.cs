using Unity.Entities;

namespace Beast.Radar.Sprite
{
    public struct SpriteComponent : IComponentData
    {
        public ushort value;

        public SpriteComponent(ushort value)
        {
            this.value = value;
        }
    }
}