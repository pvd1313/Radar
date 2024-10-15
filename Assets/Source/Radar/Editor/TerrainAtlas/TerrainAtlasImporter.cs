using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor.TerrainAtlas
{
    [SerializeField]
    public class TerrainAtlasImporter
    {
        public void MainTexture(TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.filterMode = FilterMode.Point;
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
        }

        public void ColorTexture(TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = false;
        }

        public void ConfigTexture(TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = false;
        }
    }
}