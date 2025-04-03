using System;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Text
{
    [Serializable]
    public class TextBakerConfig
    {
        public Font fontAsset;
        public int2 textureSize;
        public Color backgroundColor;
        public Color fontColor;
        public Shader fontShader;
        public float fontScale;
        public float2 vertexOffset;
    }
}