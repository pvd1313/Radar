using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Text
{
    public class TextBaker : Controller
    {
        private readonly TextGenerator _generator;
        private readonly int2 _textureSize;
        private readonly Vector3 _vertexOffset;
        private readonly Color _backgroundColor;

        private TextGenerationSettings _generatorSettings;
        private Material _fontMaterial;
        private RenderTexture _renderTexture;
        private Mesh _textMesh;
        
        public TextBaker(TextBakerConfig config)
        {
            _generator = new TextGenerator();

            _textureSize = config.textureSize;

            _vertexOffset = new Vector3(config.vertexOffset.x, config.vertexOffset.y, 0);

            _backgroundColor = config.backgroundColor;
            
            _fontMaterial = new Material(config.fontShader);
            _fontMaterial.mainTexture = config.fontAsset.material.mainTexture;
            _fontMaterial.color = config.fontColor;
            _fontMaterial.SetColor("_BackgroundColor", _backgroundColor);
            
            _generatorSettings = new TextGenerationSettings
            {
                color = Color.white,
                scaleFactor = config.fontScale,
                font = config.fontAsset,
                generateOutOfBounds = true,
                resizeTextForBestFit = false, 
                generationExtents = new Vector2(_textureSize.x, _textureSize.y),
                textAnchor = TextAnchor.LowerLeft,
            };
            
            _renderTexture = BGraphic.CreatePermRenderTexture(this, new RenderTextureData
            {
                size = _textureSize,
                antiAliasing = 1,
                filterMode = FilterMode.Point,
                name = "Text2TextureBaker_RT",
                wrapMode = TextureWrapMode.Clamp,
                format = RenderTextureFormat.ARGB32
            });

            _textMesh = BGraphic.CreateMesh(this);
            _textMesh.name = "TextBakerMesh";
        }

        public TextBakeResult Bake(string text, int fontSize)
        {
            _generatorSettings.fontSize = fontSize;
            
            _generator.Populate(text, _generatorSettings);

            Bounds2D meshBounds = _setupMesh(_generator, _textMesh);
            meshBounds.x.min = BMath.Max(0, meshBounds.x.min);
            meshBounds.y.min = BMath.Max(0, meshBounds.y.min);
            meshBounds.y.max = fontSize;
            meshBounds.x.max = BMath.Ceil(meshBounds.x.max);
            
            int2 textureOffset = new ((int)meshBounds.x.min, (int)meshBounds.y.min);
            int2 textureSize = new ((int)meshBounds.x.GetRange(), (int)meshBounds.y.GetRange());
            
            _renderMesh();

            return new TextBakeResult
            {
                texture = _renderTexture,
                offset = textureOffset,
                size = textureSize
            };
        }

        private void _renderMesh()
        {
            RenderTexture bakTexture = RenderTexture.active;

            RenderTexture.active = _renderTexture;
            
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, _textureSize.x, 0, _textureSize.y);

            _fontMaterial.SetPass(0);
            
            GL.Clear(true, true, _backgroundColor);

            Graphics.DrawMeshNow(_textMesh, Matrix4x4.identity);

            GL.PopMatrix();
            
            RenderTexture.active = bakTexture;
        }

        private Bounds2D _setupMesh(TextGenerator generator, Mesh meshToSetup)
        {
            meshToSetup.Clear();

            IList<UIVertex> uiVertices = generator.verts;

            NativeArray<float3> meshVertices = BNative.TempArray<float3>(uiVertices.Count);
            NativeArray<float2> meshUV = BNative.TempArray<float2>(uiVertices.Count);
            NativeArray<ushort> meshIndexes = BNative.TempArray<ushort>(uiVertices.Count / 4 * 6);

            Bounds2D meshBounds = default;
            
            for (int i = 0; i < meshVertices.Length; i++)
            {
                Vector3 vertex = uiVertices[i].position + _vertexOffset;
                meshVertices[i] = vertex;
                meshBounds = meshBounds.EnsureContains(vertex.XY());
                
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

            return meshBounds;
        }

        public void Dispose()
        {
            BGraphic.TryDispose(ref _renderTexture);
            BGraphic.TryDispose(ref _textMesh);
            BGraphic.TryDispose(ref _fontMaterial);
        }
    }
}