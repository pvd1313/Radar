using Beast.Claw;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public struct TerrainBiomeRenderTask : IValidatable
    {
        public int2 screenSize;
        public float2 renderPosition;
        public float viewDistance;
        public Texture bakedTerrain;
        public bool useCameraScalingConstraint;
        public bool useRenderPositionGridSnap;
        
        public RenderTexture outputBiomeTexture;
        
        public string Validate()
        {
            if (screenSize.x <= 0 || screenSize.y <= 0)
            {
                return "Invalid screenSize: " + screenSize;
            }

            if (viewDistance <= 0)
            {
                return "Invalid viewDistance: " + viewDistance;
            }

            if (bakedTerrain == null)
            {
                return "bakedTerrain is null";
            }

            if (outputBiomeTexture == null)
            {
                return "outputBiomeTexture is null";
            }

            return null;
        }
    }
}