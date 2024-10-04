namespace Beast.Radar.UI
{
    public struct GenerationMapData
    {
        public int seed;
        public int size;
        public float forest;
        public float water;

        public override string ToString()
        {
            return $"seed {seed} | size {seed} | forest {forest} | water {water}";
        }
    }
}