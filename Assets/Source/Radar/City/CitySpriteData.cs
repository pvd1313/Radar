using System;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Radar.City
{
    [Serializable]
    public struct CitySpriteData
    {
        public Texture2D icon;
        public IntMinMax citySize;
        public FloatMinMax displayViewDistance;
    }
}