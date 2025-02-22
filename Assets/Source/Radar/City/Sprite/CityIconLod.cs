using System;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Radar.City
{
    [Serializable]
    public struct CityIconLod
    {
        public FloatMinMax viewDistance;
        [CanBeNull] public Texture2D minorCityIcon;
        [CanBeNull] public Texture2D majorCityIcon;
    }
}