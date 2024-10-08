using System;
using Beast.Claw.Mathematics;
using Unity.Mathematics;

namespace Beast.Radar.Cinematic
{
    [Serializable]
    public struct IsometricCameraRotation
    {
        public float roll;
        public float pitch;
        
        public static IsometricCameraRotation operator *(IsometricCameraRotation rotation, float scale)
        {
            return new IsometricCameraRotation
            {
                pitch = rotation.pitch * scale,
                roll = rotation.roll * scale
            };
        }
        
        public static float2x2 GetScaleMatrix(float pitchRad)
        {
            return float2x2.Scale(1, BMath.Cos(pitchRad));
        }

        public static float2x2 GetRotationMatrix(float rollRad)
        {
            return float2x2.Rotate(rollRad);
        }
    }
}