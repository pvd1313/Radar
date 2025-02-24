using Unity.Entities;

namespace Beast.Radar.City
{
    public struct CityIDComponent : IComponentData
    {
        public readonly ushort id;
        
        public CityIDComponent(ushort id)
        {
            this.id = id;
        }
    }
}