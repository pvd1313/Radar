using UnityEditor;
using UnityEngine;

namespace Beast.Radar.Editor.Atlas
{
    [SerializeField]
    public class TextureImporter
    {
        public void MainTexture(UnityEditor.TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.filterMode = FilterMode.Point;
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
        }

        public void ColorTexture(UnityEditor.TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = false;
        }

        public void ConfigTexture(UnityEditor.TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Point;
            importer.textureType = TextureImporterType.Default;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = false;
        }
        
        public void IndexTexture(UnityEditor.TextureImporter importer)
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