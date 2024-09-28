using System;
using UnityEngine;

namespace Beast.Radar.CG
{
    public partial class CGLibrary
    {
        [Serializable]
        public class Debug
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            public Texture2D bakedWorldMap;

            public Texture2D worldGrid;
#endif
        }
    }
}