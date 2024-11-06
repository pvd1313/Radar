using Beast.Claw.Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beast.Claw.Graphic
{
    partial class BGraphic
    {
        public static CommandBuffer GetTempBuffer(IController owner)
        {
            return CommandBufferPool.Get(owner.Meta.name);
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