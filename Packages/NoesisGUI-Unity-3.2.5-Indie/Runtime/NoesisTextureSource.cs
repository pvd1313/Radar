using System;
using System.Runtime.InteropServices;

namespace Noesis
{
    [Obsolete("Dont use! It crashes on Vulkan!")]
    public partial class TextureSource
    {
        [Obsolete("Dont use! It crashes on Vulkan!")]
        public TextureSource(UnityEngine.Texture texture): this(Texture.WrapTexture(texture))
        {
            throw new NotSupportedException("TextureSource crashes Vulkan!");
        }

        [Obsolete("Dont use! It crashes on Vulkan!")]
        public TextureSource(UnityEngine.Texture2D texture): this(Texture.WrapTexture(texture))
        {
            throw new NotSupportedException("TextureSource crashes Vulkan!");
        }

        internal static void SetTexture(IntPtr cPtr, UnityEngine.Texture tex)
        {
            if (tex != null)
            {
                IntPtr nativePtr = Texture.EnsureNativePointer(tex);
                int numLevels = tex is UnityEngine.Texture2D ? ((UnityEngine.Texture2D)tex).mipmapCount : 1;
                Noesis_TextureSource_SetTexture(cPtr, nativePtr, tex.width, tex.height, numLevels);
            }
        }

        #region Imports
        [DllImport(Library.Name)]
        static extern void Noesis_TextureSource_SetTexture(IntPtr cPtr, IntPtr texture,
            int width, int height, int numLevels);
        #endregion
    }
}