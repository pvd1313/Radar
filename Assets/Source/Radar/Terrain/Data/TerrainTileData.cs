using System;
using Beast.Claw;
using Beast.Claw.Math;
using UnityEngine;

namespace Beast.Radar.Terrain
{
    [Serializable]
    public class TerrainTileData : IValidatable
    {
        public Texture2D texture;
        public string name;
        public Color averageColor;
        public float yCropHeight;
        public float yBlendCenter;
        public FloatMinMax waterRange;
        public FloatMinMax forestRange;
        public int index;
        
        public string Validate()
        {
            if (texture == null)
            {
                return "Texture can not be null";
            }
            
#if UNITY_EDITOR
            string texturePath = UnityEditor.AssetDatabase.GetAssetPath(texture);
            UnityEditor.TextureImporter importer = (UnityEditor.TextureImporter) UnityEditor.AssetImporter.GetAtPath(texturePath);

            if (importer.textureCompression != UnityEditor.TextureImporterCompression.Uncompressed)
            {
                return "Texture is compressed. This will lead to artifacts when atlas is rendered.";
            }
#endif

            if (yCropHeight < 0 || yCropHeight > 1)
            {
                return $"Invalid crop height ({yCropHeight})";
            }

            if (yBlendCenter < 0 || yBlendCenter > 1)
            {
                return $"Invalid blend center ({yBlendCenter})";
            }

            if (index < 0)
            {
                return $"Invalid index " + index;
            }

            if (!waterRange.IsValidRange() || !waterRange.IsNormalized())
            {
                return $"Invalid spawn water range: {waterRange}";
            }

            if (!forestRange.IsValidRange() || !forestRange.IsNormalized())
            {
                return $"Invalid spawn forest range: {forestRange}";
            }

            if (forestRange.GetRange() + waterRange.GetRange() < BMath.EPSILON)
            {
                return $"Invalid spawn range. Must have range to being able to spawn tile";
            }

            return null;
        }
    }
}