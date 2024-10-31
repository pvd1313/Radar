using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public interface ITerrainBakedFbm : IReady
    {
        public Texture Texture { get; }
    }
}