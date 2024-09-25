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

        public static RenderTexture NewRenderTexture<T>(T owner, RenderTextureParameters input)
        {
            RenderTexture texture = new (
                input.size.x,
                input.size.y,
                input.depth,
                input.format,
                input.mipCount)
            {
                name = BeastTypeStorage<T>.FullNameWithPostfix(input.name),
                filterMode = input.filterMode
            };

            return texture;
        }

        public static Material NewMaterial<T>(T owner, MaterialParameters input)
        {
            return new Material(input.shader)
            {
                name = BeastTypeStorage<T>.FullNameWithPostfix(input.name),
                mainTexture = input.mainTexture
            };
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