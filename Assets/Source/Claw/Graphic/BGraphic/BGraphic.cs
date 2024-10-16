using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Beast.Claw.Graphic
{
    [DomainInitialize]
    public static partial class BGraphic
    {
        private static readonly HashSet<RenderTexture> _permanentTextures;
        private static readonly HashSet<RenderTexture> _temporaryTextures;
        
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

                _permanentTextures = new HashSet<RenderTexture>();

                _temporaryTextures = new HashSet<RenderTexture>();
            }
            catch (Exception e)
            {
                BDebug.StaticLogException(e);
            }
        }

        public static RenderTexture CreatePermanentRenderTexture<T>(T owner, NewPermaRenderTextureData input)
        {
            input.ValidateThrowException();
            
            RenderTexture texture = new (
                input.size.x,
                input.size.y,
                input.depth,
                input.format,
                input.mipCount)
            {
                name = TypeStorage<T>.FullNameWithPostfixOrFullName(input.name),
                filterMode = input.filterMode,
                wrapMode = input.wrapMode
            };

            _permanentTextures.Add(texture);

            return texture;
        }

        public static RenderTexture CreateTemporaryRenderTexture<T>(T owner, NewTempRenderTextureData input)
        {
            input.ValidateThrowException();
            
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

            _temporaryTextures.Add(texture);

            return texture;
        }

        public static Material CreateMaterial<TOwner>(TOwner owner, MaterialParameters input) where TOwner : class
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            input.ValidateThrowException();
            
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
                name = TypeStorage<TOwner>.FullName
            };
        }

        public static Texture2D CreateTexture2D<TOwner>(TOwner owner, Texture2DParameters input)
        {
            owner.ValidateNotNullThrowException(nameof(owner));
            input.ValidateThrowException();

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
                targetTexture.ReadPixels(rect, 0, 0, targetTexture.mipmapCount > 0);
                
                targetTexture.Apply();
            }
            finally
            {
                RenderTexture.active = renderTextureBak;
            }
        }

        public static void DrawTexture2DToRenderTexture(Texture2DToRenderTexture input)
        {
            input.ValidateThrowException();

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

        public static void DrawMesh(DrawMeshParameters input)
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