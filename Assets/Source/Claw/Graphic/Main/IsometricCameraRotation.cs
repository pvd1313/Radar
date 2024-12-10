using System;
using Beast.Claw.Math;
using Unity.Mathematics;

namespace Beast.Claw.Graphic
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

        public bool IsZero()
        {
            return BMath.Approximately(roll, 0) && BMath.Approximately(pitch, 0);
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