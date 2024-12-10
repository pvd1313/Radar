using System.Diagnostics;
using System.IO;
using Beast.Claw.Graphic;
using Beast.Claw.Math.CCL;
using Beast.Radar.Terrain;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Beast.Radar.City
{
    public class DebugBiomeCCLExporter
    {
        [Conditional("DEVELOPMENT")]
        public void Export(AveragePixelCountTexture<TerrainCell> texture, string path)
        {
            int textureSizeX = texture.cellCountX;
            
            CCLTextureJob<BiomeCCLTexture> recognitionJob = new()
            {
                inTexture = new BiomeCCLTexture(texture.texture, textureSizeX),
                outLabeledTexture = new NativeArray<ushort>(textureSizeX * textureSizeX, Allocator.TempJob)
            };
			
            recognitionJob.Schedule().Complete();

            Texture2D cclTexture = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                isLinear = true,
                createInitialized = false,
                size = textureSizeX,
                filterMode = FilterMode.Point,
                mipChain = false,
                format = TextureFormat.RGB24
            });
            
            for (int y = 0; y < textureSizeX; y++)
            for (int x = 0; x < textureSizeX; x++)
            {
                ushort label = recognitionJob.outLabeledTexture[y * textureSizeX + x];

                Color color;
                
                if (label == 0)
                {
                    color = Color.black;
                }
                else
                {
                    Random.InitState(label);
                    color = Random.ColorHSV(0.5f, 1, 0.5f, 1, 0.5f, 1f);
                }
             
                cclTexture.SetPixel(x, y, color);
            }

            cclTexture.Apply();

            File.WriteAllBytes(path, cclTexture.EncodeToPNG());
			
            recognitionJob.outLabeledTexture.Dispose();
            BGraphic.Dispose(cclTexture);
        }
    }
}