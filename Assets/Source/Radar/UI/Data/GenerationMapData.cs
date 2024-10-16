namespace Beast.Radar.UI
{
    public struct GenerationMapData
    {
        public int seed;
        public int size;
        public float forest;
        public float water;
        public float biomeSize;

        public override string ToString()
        {
            return $"seed {seed} | size {size} | forest {forest} | water {water} | biome {biomeSize}";
        }
    }
}