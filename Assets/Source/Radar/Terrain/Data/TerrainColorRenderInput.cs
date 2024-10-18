using Beast.Radar.Cinematic;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public struct TerrainColorRenderInput
    {
        public IsometricCameraRotation cameraRotationRad;
        public float cameraViewDistance;
        public int2 screenSize;
        public float2 biomeTextureUVOffset;
        public float biomeViewDistance;
        public Texture biomeTexture;
        public bool outputToCamera;
        public RenderTexture outputToTexture;
    }
}