using UnityEngine;
using UnityEngine.Rendering;

namespace Beast.Claw.CG
{
    public static class BeastCG
    {
        public static CommandBuffer GetTempBufferFor<T>()
        {
            return CommandBufferPool.Get(typeof(T).FullName);
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
                input.mipCount);

            texture.name = typeof(T).FullName + "(" + input.name + ")";

            return texture;
        }

        public static Material NewMaterial<T>(T owner, MaterialParameters input)
        {
            return new Material(input.shader)
            {
                name = typeof(T).FullName + "(" + input.name + ")",
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
                input.camera,
                input.layer,
                input.materialProperties);
        }
    }
}