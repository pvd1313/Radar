using System;
using Unity.Mathematics;

namespace Beast.Radar.UI.Player
{
    [Serializable]
    public struct PlayerViewData
    {
        public XamlAsset asset;
        public float viewSize;
        public float2 offset;
    }
}