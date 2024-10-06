using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Beast.Claw.CG
{
    [BeastDomainInitialize]
    public static class BeastCG
    {
        private static readonly StringBuilder _textBuilder;
        private static readonly Dictionary<FormatUsage, string> _formatUsage2String;
        private static readonly Dictionary<GraphicsFormat, string> _graphicsFormat2String;
        private static readonly HashSet<RenderTexture> _permanentTextures;
        private static readonly HashSet<RenderTexture> _temporaryTextures;
        
        static BeastCG()
        {
            try
            {
                _textBuilder = new StringBuilder();

                _formatUsage2String = BeastEnumStorage<FormatUsage>.Values
                    .Distinct()
                    .ToDictionary(x => x, x => x.ToString());

                _graphicsFormat2String = BeastEnumStorage<GraphicsFormat>.Values
                    .Distinct()
                    .ToDictionary(x => x, x => x.ToString());

                _permanentTextures = new HashSet<RenderTexture>();

                _temporaryTextures = new HashSet<RenderTexture>();
            }
            catch (Exception e)
            {
                BDebug.LogStaticException(e, "BeastCG cctor");
            }
        }
        
        public static CommandBuffer GetTempBufferFor<T>()
        {
            return CommandBufferPool.Get(BeastTypeStorage<T>.FullName);
        }

        public static void ReleaseBuffer(CommandBuffer buffer)
        {
            CommandBufferPool.Release(buffer);
        }

        public static void ExecuteBufferAsync(CommandBuffer buffer, ComputeQueueType queueType)
        {
            Graphics.ExecuteCommandBufferAsync(buffer, queueType);
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
                name = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(input.name),
                filterMode = input.filterMode,
                wrapMode = input.wrapMode
            };

            _permanentTextures.Add(texture);

            return texture;
        }

        public static void ReleaseRenderTexture(RenderTexture renderTexture)
        {
            BAssert.ArgumentNotNull(renderTexture, nameof(renderTexture));
            
            if (_permanentTextures.Remove(renderTexture))
            {
                renderTexture.Release();
                return;
            }

            if (_temporaryTextures.Remove(renderTexture))
            {
                RenderTexture.ReleaseTemporary(renderTexture);
                return;
            }
            
            BDebug.LogStaticError($"Failed to release render texture '{renderTexture.name}'. As it was not found in list.");
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

        public static Material CreateMaterial<T>(T owner, MaterialParameters input)
        {
            input.ValidateThrowException();
            
            return new Material(input.shader)
            {
                name = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(input.name),
            };
        }

        public static void ReleaseMaterial(Material material)
        {
            BAssert.ArgumentNotNull(material, nameof(material));
            
            Object.Destroy(material);
        }

        public static Texture2D CreateTexture2DFromRenderTexture<T>(T owner, Texture2DParametersA input)
        {
            input.ValidateThrowException();

            Texture2D texture = new (
                input.sourceTexture.width,
                input.sourceTexture.height,
                input.format,
                input.mipChain,
                input.isLinear,
                true)
            {
                name = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(input.name)
            };
            
            RenderTexture renderTextureBak = RenderTexture.active;
            
            try
            {
                RenderTexture.active = input.sourceTexture;
                
                Rect rect = new (0, 0, input.sourceTexture.width, input.sourceTexture.height);
                texture.ReadPixels(rect, 0, 0, input.mipChain);
                
                texture.Apply();
            }
            finally
            {
                RenderTexture.active = renderTextureBak;
            }

            return texture;
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

        public static string GetSupportedGraphicsFormats()
        {
            _textBuilder.Clear();

            _textBuilder.AppendLine("Supported Graphics Formats: ");

            foreach (FormatUsage formatUsage in BeastEnumStorage<FormatUsage>.Values)
            {
                _textBuilder.Append("- ");
                _textBuilder.Append(_formatUsage2String[formatUsage]);
                _textBuilder.Append(": ");

                foreach (GraphicsFormat graphicsFormat in BeastEnumStorage<GraphicsFormat>.Values)
                {
                    _textBuilder.Append(_graphicsFormat2String[graphicsFormat]);
                    _textBuilder.Append(", ");
                }

                _textBuilder.AppendLine();
                _textBuilder.AppendLine();
            }

            return _textBuilder.ToString();
        }
    }
}