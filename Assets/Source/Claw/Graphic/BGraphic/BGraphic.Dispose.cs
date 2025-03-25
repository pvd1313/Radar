using Beast.Claw.Debug;
using Beast.Claw.Validation;
using UnityEngine;
using UnityEngine.Rendering;

namespace Beast.Claw.Graphic
{
    public static partial class BGraphic
    {
        public static void Dispose(RenderTexture renderTexture)
        {
            renderTexture.ValidateNotNullThrowException(nameof(renderTexture));

            if (_permRenderTexture.Remove(renderTexture))
            {
                renderTexture.Release();
                return;
            }

            if (_tempRenderTexture.Remove(renderTexture))
            {
                RenderTexture.ReleaseTemporary(renderTexture);
                return;
            }

            BDebug.StaticLogError($"RT dispose failed '{renderTexture.name}'. (not found in list)");
        }
        
        public static void Dispose(Material material)
        {
            material.ValidateNotNullThrowException(nameof(material));

            Object.Destroy(material);
        }
        
        public static void Dispose(Mesh mesh)
        {
            mesh.ValidateNotNullThrowException(nameof(mesh));

            Object.Destroy(mesh);
        }
        
        public static void Dispose(Texture2D texture)
        {
            texture.ValidateNotNullThrowException(nameof(texture));

            _texture2D.Remove(texture);

            Object.Destroy(texture);
        }
        
        public static void DisposeTempBuffer(CommandBuffer buffer)
        {
            CommandBufferPool.Release(buffer);
        }

        public static bool TryDispose(ref Texture2D texture)
        {
            if (texture == null)
            {
                return false;
            }

            _texture2D.Remove(texture);

            Object.Destroy(texture);

            texture = null;

            return true;
        }

        public static bool TryDispose(ref Mesh mesh)
        {
            if (mesh == null)
            {
                return false;
            }

            Object.Destroy(mesh);

            mesh = null;
            
            return true;
        }

        public static bool TryDispose(ref RenderTexture renderTexture)
        {
            if (renderTexture == null)
            {
                return false;
            }

            if (_permRenderTexture.Remove(renderTexture))
            {
                renderTexture.Release();
                renderTexture = null;
                return true;
            }

            if (_tempRenderTexture.Remove(renderTexture))
            {
                RenderTexture.ReleaseTemporary(renderTexture);
                renderTexture = null;
                return true;
            }

            BDebug.StaticLogError($"RT dispose failed '{renderTexture.name}'. (not found in list)");

            return false;
        }

        public static bool TryDispose(ref Material material)
        {
            if (material == null)
            {
                return false;
            }

            Object.Destroy(material);
            material = null;

            return true;
        }
    }
}