using System;
using UnityEngine;

namespace Beast.Radar
{
    public partial class BeastGraphicLibrary
    {
        [Serializable]
        public class Debug
        {
#if BEAST_DEVELOPMENT
            public Texture2D bakedWorldMap;

            public Texture2D worldGrid;
#endif
        }
    }
}