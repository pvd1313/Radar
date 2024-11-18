using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;
using Beast.Claw.Validation;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Beast.Claw.Graphic
{
    [DomainInitialize]
    public static partial class BGraphic
    {
        private static readonly HashSet<RenderTexture> PERMANENT_TEXTURES;
        private static readonly HashSet<RenderTexture> TEMPORARY_TEXTURES;
        
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

                PERMANENT_TEXTURES = new HashSet<RenderTexture>();
                TEMPORARY_TEXTURES = new HashSet<RenderTexture>();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        public static RenderTexture CreatePermRenderTexture<TOwner>(TOwner owner, RenderTextureData input)
        {
            input.ValidateThrowException(nameof(input));
            
            RenderTexture texture = new (
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

            PERMANENT_TEXTURES.Add(texture);

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

            TEMPORARY_TEXTURES.Add(texture);

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

        public static Texture2D CreateTexture2D<TOwner>(TOwner owner, Texture2DParameters input) 
            where TOwner: class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            input.ValidateThrowException(nameof(input));

            return new Texture2D(
                input.size.x,
                input.size.y,
                input.format,
                input.mipChain,
                input.isLinear,
                input.createInitialized)
            {
                name = TypeStorage<TOwner>.FullNameWithPostfixOrFullName(input.name),
                filterMode = input.filterMode,
                wrapMode = input.wrapMode
            };
        }

        public static void BlitRenderTextureTo2D(RenderTexture sourceTexture, Texture2D targetTexture)
        {
            sourceTexture.ValidateNotNullThrowException(nameof(sourceTexture));
            targetTexture.ValidateNotNullThrowException(nameof(targetTexture));
            
            RenderTexture renderTextureBak = RenderTexture.active;
            
            try
            {
                RenderTexture.active = sourceTexture;
                
                Rect rect = new (0, 0, targetTexture.width, targetTexture.height);
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

            Rect rect = new (
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
    }
}