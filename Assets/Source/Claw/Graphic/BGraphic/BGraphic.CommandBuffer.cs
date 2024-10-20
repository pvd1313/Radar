using Beast.Claw.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beast.Claw.Graphic
{
    partial class BGraphic
    {
        public static CommandBuffer GetTempBuffer<TOwner>(TOwner owner)
        {
            return CommandBufferPool.Get(TypeStorage<TOwner>.FULL_NAME);
        }

        public static void ExecuteBuffer(CommandBuffer buffer)
        {
            Graphics.ExecuteCommandBuffer(buffer);
        }

        public static void ExecuteBufferAsync(CommandBuffer buffer, ComputeQueueType queueType)
        {
            Graphics.ExecuteCommandBufferAsync(buffer, queueType);
        }
    }
}