using System;
using UnityEngine;

namespace Beast.Radar
{
    public partial class GraphicLibrary
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