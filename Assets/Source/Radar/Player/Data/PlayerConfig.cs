using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Mathematics;
using Beast.Radar.Terrain;

namespace Beast.Radar.Player
{
    public class PlayerConfig : ScriptableConfig
    {
        public float moveSpeed;
        public TileSlowdown[] tileSpeedModifiers;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
        
        public ByteArray16 TileSpeedModifiersToTileCost()
        {
            ByteArray16 byteArray = new();
            
            int count = BMath.Min(tileSpeedModifiers.Length, 16);
            
            for (int i = 0; i < count; i++)
            {
                TileSlowdown modifier = tileSpeedModifiers[i];
                byte speedValue = (byte)((1f - modifier.speedMultiplier) * 255);
                byteArray.SetByte((byte)modifier.biome, speedValue);
            }

            return byteArray;
        }
    }
}