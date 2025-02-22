using System.Collections.Generic;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.U2D;

namespace Beast.Claw.Text.Baking
{
    public class TextBaker : Actor
    {
        private readonly TextGenerator _generator;
        private readonly TextGenerationSettings _generatorSettings;
        private readonly Font _font;

        private Texture2D _texture2D;
        private RenderTexture _renderTexture;
        private Mesh _textMesh;
        
        public TextBaker(TextBakerConfig config)
        {
            _generator = new TextGenerator();

            _font = config.fontAsset;
            
            _generatorSettings = new TextGenerationSettings
            {
                fontSize = config.fontSize,
                color = Color.white,
                scaleFactor = 1f,
                font = config.fontAsset,
                generateOutOfBounds = true,
                resizeTextForBestFit = false, 
                generationExtents = new Vector2(256, 16),
                textAnchor = TextAnchor.LowerLeft
            };
            
            _texture2D = BGraphic.CreateTexture2D(this, new Texture2DParameters
            {
                isLinear = true,
                createInitialized = false,
                filterMode = FilterMode.Point,
                format = TextureFormat.RGB24,
                mipChain = false,
                name = "Text2TextureBaker_2D",
                size = new int2(128, 16),
                wrapMode = TextureWrapMode.Clamp
            });
            
            _renderTexture = BGraphic.CreatePermRenderTexture(this, new RenderTextureData
            {
                size = new int2(128, 16),
                antiAliasing = 1,
                filterMode = FilterMode.Point,
                name = "Text2TextureBaker_RT",
                wrapMode = TextureWrapMode.Clamp,
                format = RenderTextureFormat.R8
            });

            _textMesh = new Mesh
            {
                name = "TextBakerMesh"
            };
        }

        public void Bake(string text)
        {
            _generator.Populate(text, _generatorSettings);

            _setupMesh(_generator, _textMesh);
            
            _renderMesh();
        }

        private void _renderMesh()
        {
            RenderTexture bakTexture = RenderTexture.active;

            RenderTexture.active = _renderTexture;
            
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, 128, 0, 16);

            _font.material.SetPass(0);

            Graphics.DrawMeshNow(_textMesh, Matrix4x4.identity);
            
            GL.PopMatrix();
            RenderTexture.active = bakTexture;
        }

        private static void _setupMesh(TextGenerator generator, Mesh meshToSetup)
        {
            meshToSetup.Clear();

            IList<UIVertex> uiVertices = generator.verts;

            NativeArray<float3> meshVertices = BNative.TempArray<float3>(uiVertices.Count);
            NativeArray<float2> meshUV = BNative.TempArray<float2>(uiVertices.Count);
            NativeArray<ushort> meshIndexes = BNative.TempArray<ushort>(uiVertices.Count / 4 * 6);
            
            for (int i = 0; i < meshVertices.Length; i++)
            {
                meshVertices[i] = uiVertices[i].position;
                meshUV[i] = ((float4)uiVertices[i].uv0).xy;
            }
            
            for (int i = 0; i < meshVertices.Length / 4; i++)
            {
                int vertexIndex = i * 4;
                int triangleIndex = i * 6;

                meshIndexes[triangleIndex + 0] = (ushort)(vertexIndex + 0);
                meshIndexes[triangleIndex + 1] = (ushort)(vertexIndex + 1);
                meshIndexes[triangleIndex + 2] = (ushort)(vertexIndex + 3);
                
                meshIndexes[triangleIndex + 3] = (ushort)(vertexIndex + 3);
                meshIndexes[triangleIndex + 4] = (ushort)(vertexIndex + 1);
                meshIndexes[triangleIndex + 5] = (ushort)(vertexIndex + 2);
            }

            meshToSetup.SetVertices(meshVertices);
            meshToSetup.SetUVs(0, meshUV);
            meshToSetup.SetIndices(meshIndexes, MeshTopology.Triangles, 0, false);

            meshVertices.Dispose();
            meshUV.Dispose();
            meshIndexes.Dispose();
        }

        public void Dispose()
        {
            BGraphic.TryDispose(ref _texture2D);
            BGraphic.TryDispose(ref _renderTexture);
            BGraphic.TryDispose(ref _textMesh);
        }
    }
}