using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct World2ClipMatrix
    {
        public float2 cameraPosition;
        public float cameraViewDistance;
        public float2 screenSize;
        public IsometricCameraRotation cameraRotationRad;

        public void Apply(Material material)
        {
            material.SetFloat2("_RenderCenter", cameraPosition);
            material.SetFloat("_RenderDistance", cameraViewDistance);
            material.SetFloat2("_ScreenSize", screenSize);

            if (cameraRotationRad.IsZero())
            {
                material.SetFloat2x2("_ViewMatrix0", float2x2.identity);
                material.SetFloat2x2("_ViewMatrix1", float2x2.identity);
                material.SetFloat("_ProjectionScale", 2f);
            }
            else
            {
                float rollCos = BMath.Cos(cameraRotationRad.roll);
                float pitchCos = BMath.Cos(cameraRotationRad.pitch);
            
                float2x2 rotation = float2x2.Rotate(cameraRotationRad.roll * -1);
                float2x2 scale = float2x2.Scale(1, pitchCos);
            
                material.SetFloat2x2("_ViewMatrix0", rotation);
                material.SetFloat2x2("_ViewMatrix1", scale);
                material.SetFloat("_ProjectionScale", 1f / (rollCos * pitchCos));
            }
        }
    }
}