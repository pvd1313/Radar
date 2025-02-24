using System;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Radar.City
{
    [Serializable]
    public struct CityMarkerLod
    {
        public FloatMinMax viewDistance;
        [CanBeNull] public Texture2D minorIcon;
        [CanBeNull] public Texture2D majorIcon;
        public int minorLabelSize;
        public int majorLabelSize;
    }
}