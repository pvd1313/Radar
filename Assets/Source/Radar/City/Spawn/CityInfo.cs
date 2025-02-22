using Beast.Claw.Math;

namespace Beast.Radar.City
{
    public struct CityInfo
    {
        public short2 position;
        public byte size;
        
        public override string ToString()
        {
            return $"{{position: ({position.x}, {position.y}), s{size})}}";
        }
    }
}