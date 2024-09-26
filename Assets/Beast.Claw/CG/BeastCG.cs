using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Beast.Claw.Common;
using Beast.Claw.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Beast.Claw.CG
{
    public static class BeastCG
    {
        private static readonly StringBuilder TextBuilder;
        private static readonly Dictionary<FormatUsage, string> FormatUsage2String;
        private static readonly Dictionary<GraphicsFormat, string> GraphicsFormat2String;

        static BeastCG()
        {
            try
            {
                TextBuilder = new StringBuilder();

                FormatUsage2String = BeastEnumStorage<FormatUsage>.Values
                    .Distinct()
                    .ToDictionary(x => x, x => x.ToString());

                GraphicsFormat2String = BeastEnumStorage<GraphicsFormat>.Values
                    .Distinct()
                    .ToDictionary(x => x, x => x.ToString());
            }
            catch (Exception e)
            {
                BeastLog.StaticException(e, "BeastCG cctor");
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

        public static RenderTexture NewRenderTexturePermanent<T>(T owner, NewPersistentRenderTextureData input)
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

            return texture;
        }

        public static void ReleaseTemporaryRenderTexture(RenderTexture renderTexture)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        public static RenderTexture NewRenderTextureTemporary<T>(T owner, NewTemporaryRenderTextureData input)
        {
            input.ValidateThrowException();

            return RenderTexture.GetTemporary(
                input.size.x,
                input.size.y,
                input.depth,
                input.format,
                input.readWrite,
                input.antiAliasing,
                input.memoryless,
                input.vrUsage);
        }

        public static Material NewMaterial<T>(T owner, MaterialParameters input)
        {
            input.ValidateThrowException();
            
            return new Material(input.shader)
            {
                name = BeastTypeStorage<T>.FullNameWithPostfixOrFullName(input.name),
            };
        }

        public static Texture2D NewTexture2DFromRenderTexture<T>(T owner, Texture2DParametersA input)
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
            TextBuilder.Clear();

            TextBuilder.AppendLine("Supported Graphics Formats: ");

            foreach (FormatUsage formatUsage in BeastEnumStorage<FormatUsage>.Values)
            {
                TextBuilder.Append("- ");
                TextBuilder.Append(FormatUsage2String[formatUsage]);
                TextBuilder.Append(": ");

                foreach (GraphicsFormat graphicsFormat in BeastEnumStorage<GraphicsFormat>.Values)
                {
                    TextBuilder.Append(GraphicsFormat2String[graphicsFormat]);
                    TextBuilder.Append(", ");
                }

                TextBuilder.AppendLine();
                TextBuilder.AppendLine();
            }

            return TextBuilder.ToString();
        }
    }
}