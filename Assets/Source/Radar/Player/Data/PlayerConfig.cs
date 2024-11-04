using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Terrain;
using UnityEngine;

namespace Beast.Radar.Player
{
    public class PlayerConfig : ScriptableConfig
    {
        public float moveSpeed;
        [SerializeField] private TileSlowdown[] tileSpeedModifiers;
        
        public override IEnumerable<IConfig> Bind()
        {
            yield break;
        }
        
        public ByteArray16 GetTileSpeedModifiers()
        {
            ByteArray16 byteArray = new ByteArray16();
            
            byte[] values = new byte[tileSpeedModifiers.Length];

            for (int i = 0; i < tileSpeedModifiers.Length; i++)
            {
                TileSlowdown modifier = tileSpeedModifiers[i];
                int index = (int)modifier.biome;
                byte speedValue = (byte)((1f - modifier.speedMultiplier) * 255);
                values[index] = speedValue;
                byteArray.SetByte(i, values[i]);
            }

            return byteArray;
        }
    }
}