using System;

namespace Beast.Radar.City
{
    [Serializable]
    public struct RoadRenderLODConfig
    {
        public float cellSize;
        public bool isGlobalLOD;
        public float segmentLength;
        public float activationDistance;
    }
}