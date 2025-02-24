using Unity.Entities;

namespace Beast.Radar.City
{
    public struct CityLabelCommand
    {
        public Entity targetEntity;
        public ECityLabelCommandType commandType;
        public ushort spriteID;
        public byte labelSize;
    }
}