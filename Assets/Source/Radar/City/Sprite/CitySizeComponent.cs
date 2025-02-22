using Unity.Entities;

namespace Beast.Radar.City
{
    public struct CityComponent : IComponentData
    {
        public ushort id;
        
        public CityComponent(ushort id)
        {
            this.id = id;
        }
    }
}