using Unity.Collections;

namespace Beast.Radar.Sprite
{
    public interface ISpriteAtlas
    {
        public NativeSlice<SpriteInfo> Sprites { get; }
        
        ushort CreateSprite(SpriteSetup spriteSetup);
        
        void ReleaseSprite(ushort index);

        void UpdateAtlas();
    }
}