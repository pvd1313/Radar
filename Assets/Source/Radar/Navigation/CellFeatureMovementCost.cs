using System;
using Beast.Radar.Terrain;

namespace Beast.Radar.Navigation
{
    [Serializable]
    public struct CellFeatureMovementCost
    {
        public ETerrainFeature feature;
        public byte cost;
    }
}