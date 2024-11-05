using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Mathematics;
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
            
            int count = BMath.Min(tileSpeedModifiers.Length, 16);
            
            for (int i = 0; i < count; i++)
            {
                TileSlowdown modifier = tileSpeedModifiers[i];
                int index = (int)modifier.biome;
                byte speedValue = (byte)((1f - modifier.speedMultiplier) * 255);
                byteArray.SetByte(i, speedValue);
            }

            return byteArray;
        }
    }
}