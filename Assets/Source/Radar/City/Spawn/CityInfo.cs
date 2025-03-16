using Beast.Claw.Math;

namespace Beast.Radar.City
{
    public struct CityInfo
    {
        public ushort2 position;
        public byte size;

        public bool IsMajor()
        {
            return CitySpawner.MAJOR_CITY_SIZE.Contains(size);
        }
        
        public override string ToString()
        {
            return $"{{position: ({position.x}, {position.y}), s{size})}}";
        }
    }
}