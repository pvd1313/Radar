using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beast.Claw.Attributes;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Math;
using Beast.Claw.Reflection;
using Beast.Claw.Validation;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Beast.Claw.Graphic
{
    [StaticUtility]
    [InitializedWithDomain]
    public static partial class BGraphic
    {
        private static readonly HashSet<RenderTexture> _permRenderTexture;
        private static readonly HashSet<RenderTexture> _tempRenderTexture;
        private static readonly HashSet<Texture2D> _texture2D;
        private static readonly int2[] _dataTextureSizeTable;

        static BGraphic()
        {
            try
            {
                _textBuilder = new StringBuilder();

                _formatUsage2String = EnumStorage<FormatUsage>.Values
                    .Distinct()
                    .ToDictionary(x => x, x => x.ToString());

                _graphicsFormat2String = EnumStorage<GraphicsFormat>.Values
                    .Distinct()
                    .ToDictionary(x => x, x => x.ToString());

                _permRenderTexture = new HashSet<RenderTexture>();
                _tempRenderTexture = new HashSet<RenderTexture>();
                _texture2D = new HashSet<Texture2D>();

                _dataTextureSizeTable = new int2[]
                {
                    new(1, 1), // 1 | 2^0
                    new(2, 1), // 2 | 2^1
                    new(2, 2), // 4 | 2^2
                    new(4, 2), // 8 | 2^3
                    new(4, 4), // 16 | 2^4
                    new(8, 4), // 32 | 2^5
                    new(8, 8), // 64 | 2^6,
                    new(16, 8), // 128 | 2^7,
                    new(16, 16), // 256 | 2^8,
                    new(32, 16), // 512 | 2^9,
                    new(32, 32), // 1024 | 2^10,
                    new(64, 32), // 2048 | 2^11,
                    new(64, 64), // 4096 | 2^12,
                    new(128, 64), // 8192 | 2^13,
                    new(128, 128), // 16384 | 2^14
                    new(256, 128), // 32768 | 2^15
                    new(256, 256), // 65536 | 2^16
                };
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        public static IEnumerable<Texture> GetAllocatedTextures()
        {
            foreach (RenderTexture texture in _permRenderTexture)
            {
                yield return texture;
            }
            
            foreach (Texture2D texture in _texture2D)
            {
                yield return texture;
            }
        }

        public static int2 GetDataTextureSize(int dataCount)
        {
            return _dataTextureSizeTable[BMath.IntCeilLog2(dataCount)];
        }

        public static RenderTexture CreatePermRenderTexture<TOwner>(TOwner owner, RenderTextureData input)
        {
            input.ValidateThrowException(nameof(input));

            RenderTexture texture = new(
                input.size.x,
                input.size.y,
                input.depth,
                input.format,
                input.mipCount)
            {
                name = TypeStorage<TOwner>.FullNameWithPostfixOrFullName(input.name),
                filterMode = input.filterMode,
                wrapMode = input.wrapMode
            };

            _permRenderTexture.Add(texture);

            return texture;
        }

        public static RenderTexture CreateTempRenderTexture<TOwner>(TOwner owner, RenderTextureData input)
        {
            input.ValidateThrowException(nameof(input));

            RenderTexture texture = RenderTexture.GetTemporary(
                input.size.x,
                input.size.y,
                input.depth,
                input.format,
                input.readWrite,
                input.antiAliasing,
                input.memoryless,
                input.vrUsage);

            texture.filterMode = input.filterMode;
            texture.wrapMode = input.wrapMode;

            _tempRenderTexture.Add(texture);

            return texture;
        }

        public static Material CreateMaterial<TOwner>(TOwner owner, MaterialParameters input) where TOwner : class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            input.ValidateThrowException(nameof(input));

            return new Material(input.shader)
            {
                name = TypeStorage<TOwner>.FullNameWithPostfixOrFullName(input.name),
            };
        }

        public static Material CreateMaterial<TOwner>(TOwner owner, Shader shader) where TOwner : class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            shader.ValidateNotNullThrowException(nameof(shader));

            return new Material(shader)
            {
                name = TypeStorage<TOwner>.FULL_NAME
            };
        }

        public static Texture2D CreateTexture2D(IController owner, Texture2DParameters input)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            input.ValidateThrowException(nameof(input));
            
            Texture2D texture = new (
                input.size.x,
                input.size.y,
                input.format,
                input.mipChain,
                input.isLinear,
                input.createInitialized)
            {
                name = owner.Meta.name + "." + input.name,
                filterMode = input.filterMode,
                wrapMode = input.wrapMode
            };

            _texture2D.Add(texture);

            return texture;
        }

        public static void BlitRenderTextureTo2D(RenderTexture sourceTexture, Texture2D targetTexture)
        {
            sourceTexture.ValidateNotNullThrowException(nameof(sourceTexture));
            targetTexture.ValidateNotNullThrowException(nameof(targetTexture));

            RenderTexture renderTextureBak = RenderTexture.active;

            try
            {
                RenderTexture.active = sourceTexture;

                Rect rect = new(0, 0, targetTexture.width, targetTexture.height);
                targetTexture.ReadPixels(rect, 0, 0, false);

                targetTexture.Apply(targetTexture.mipmapCount > 0);
            }
            finally
            {
                RenderTexture.active = renderTextureBak;
            }
        }

        public static void DrawTexture2DToRenderTexture(Texture2DToRenderTexture input)
        {
            input.ValidateThrowException(nameof(input));

            RenderTexture bakTexture = RenderTexture.active;

            RenderTexture.active = input.targetTexture;

            GL.PushMatrix();

            GL.LoadPixelMatrix(0, input.targetTexture.width, input.targetTexture.height, 0);

            Rect rect = new(
                input.targetTexturePixelOffset.x,
                input.targetTexture.height - input.targetTexturePixelOffset.y - input.sourceTexture.height,
                input.sourceTexture.width,
                input.sourceTexture.height);

            Graphics.DrawTexture(rect, input.sourceTexture);

            GL.PopMatrix();

            RenderTexture.active = bakTexture;
        }

        public static void Blit(BlitParameters input)
        {
            Graphics.Blit(
                input.sourceTexture,
                input.targetTexture,
                input.material,
                input.shaderPass);
        }

        public static void DrawMeshDelayed(DrawMeshTask input)
        {
            Graphics.DrawMesh(
                input.mesh,
                input.matrix,
                input.material,
                input.layer,
                input.targetCamera,
                input.layer,
                input.materialProperties);
        }

        public static void ClearTexture(RenderTexture texture)
        {
            texture.ValidateNotNullThrowException(nameof(texture));

            RenderTexture bak = RenderTexture.active;

            RenderTexture.active = texture;
            
            GL.Clear(true, true, Color.clear);

            RenderTexture.active = bak;
        }
    }
}