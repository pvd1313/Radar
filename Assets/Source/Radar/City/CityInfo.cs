using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct CityInfo
    {
        public float2 correctedPosition;
        public float2 originalPosition;
        public ECityStatus status;
    }
}