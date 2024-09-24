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

        public static RenderTexture CreateRenderTexture<T>(T owner, RenderTextureParameters parameters)
        {
            RenderTexture texture = new (
                parameters.size.x,
                parameters.size.y,
                parameters.depth,
                parameters.format,
                parameters.mipCount);

            texture.name = typeof(T).FullName + "(" + parameters.name + ")";

            return texture;
        }

        public static Material CreateMaterial<T>(T owner, MaterialParameters parameters)
        {
            return new Material(parameters.shader)
            {
                name = typeof(T).FullName + "(" + parameters.name + ")",
                mainTexture = parameters.mainTexture
            };
        }
    }
}