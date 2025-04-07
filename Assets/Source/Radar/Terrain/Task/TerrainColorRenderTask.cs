using Beast.Claw.Graphic;
using Beast.Claw.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    public struct TerrainColorRenderTask : IValidatable
    {
        public IsometricCameraRotation cameraRotationRad;
        public float cameraViewDistance;
        public int2 screenSize;
        public float2 biomeTextureUVOffset;
        public float biomeViewDistance;
        public Texture biomeTexture;
        
        public bool outputToCamera;
        public RenderTexture outputToTexture;
        
        public string Validate()
        {
            if (cameraViewDistance <= 0)
            {
                return "Invalid cameraViewDistance: " + cameraViewDistance;
            }

            if (screenSize.x <= 0 || screenSize.y <= 0)
            {
                return "Invalid screenSize: " + screenSize;
            }

            if (biomeViewDistance <= 0)
            {
                return "Invalid biomeViewDistance: " + biomeViewDistance;
            }

            if (biomeTexture == null)
            {
                return "biomeTexture is null";
            }

            if (outputToCamera)
            {
                if (outputToTexture != null)
                {
                    return "Output set to camera, but texture is also set. Remove texture or set outputToCamera = false";
                }
            }
            else
            {
                if (outputToTexture == null)
                {
                    return "outputToTexture is null. Provide texture, or set outputToCamera = true";
                }
            }

            return null;
        }
    }
}