using Unity.Collections;

namespace Beast.Radar.Sprite
{
    public struct SpriteAtlasLine
    {
        public ushort yOffset;
        public ushort height;
        public ushort freeXOffset;
        public NativeList<ushort> sprites;

        public void Dispose()
        {
            if (sprites.IsCreated)
            {
                sprites.Dispose();
            }
        }
    }
}