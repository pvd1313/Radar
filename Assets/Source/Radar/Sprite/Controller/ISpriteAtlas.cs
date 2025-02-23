namespace Beast.Radar.Sprite
{
    public interface ISpriteAtlas
    {
        ushort EmptySpriteID { get; }
        
        ushort CreateSprite(SpriteSetup spriteSetup);
        
        void DeleteSprite(ushort index);
    }
}