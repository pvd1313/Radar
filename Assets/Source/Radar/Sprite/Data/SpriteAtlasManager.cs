using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.Sprite
{
    public class SpriteAtlasManager
    {
        public NativeList<SpriteInfo> Sprites => _sprites;
        
        private readonly int2 _atlasSize;
        private readonly List<SpriteAtlasLine> _lines;
        private readonly List<ushort> _freeSpriteIndexList;
        
        private NativeList<SpriteInfo> _sprites;
        private HashSet<ushort> _linesWithGaps;
        private ushort _freeYOffset;
        
        public SpriteAtlasManager(int2 atlasSize)
        {
            _atlasSize = atlasSize;
            _lines = new List<SpriteAtlasLine>(64);
            _freeSpriteIndexList = new List<ushort>(128);
            _linesWithGaps = new HashSet<ushort>(64);
            
            _sprites = BNative.PermList<SpriteInfo>(256);
            _sprites.Add(default);
        }

        public void Allocate(int2 spriteSize, out ushort spriteIndex, out SpriteInfo spriteInfo)
        {
            int lineHeight = _spriteHeightToLineHeight(spriteSize.y);

            int lineIndex = _findLine(spriteSize.x, lineHeight);
            
            if (lineIndex == -1)
            {
                lineIndex = _createLine((ushort) lineHeight);
            }

            int lastIndex = _freeSpriteIndexList.Count - 1;

            if (lastIndex >= 0)
            {
                spriteIndex = _freeSpriteIndexList[lastIndex];
                _freeSpriteIndexList.RemoveAt(lastIndex);
            }
            else
            {
                spriteIndex = (ushort) _sprites.Length;
                _sprites.Add(default);
            }

            SpriteAtlasLine line = _lines[lineIndex];

            line.sprites.Add(spriteIndex);

            spriteInfo = new SpriteInfo
            {
                atlasOffset = new short2(line.freeXOffset, line.yOffset),
                size = (short2)spriteSize,
                lineIndex = (byte) lineIndex
            };

            _sprites[spriteIndex] = spriteInfo;

            line.freeXOffset =  (ushort)(line.freeXOffset + spriteSize.x);

            _lines[lineIndex] = line;
        }

        public void Release(ushort spriteIndex)
        {
            SpriteInfo sprite = _sprites[spriteIndex];
            _sprites[spriteIndex] = default;
            _freeSpriteIndexList.Add(spriteIndex);

            SpriteAtlasLine line = _lines[sprite.lineIndex];
            line.sprites.RemoveValue(spriteIndex);
            _lines[sprite.lineIndex] = line;

            _linesWithGaps.Add(sprite.lineIndex);
        }

        /// <returns>8, 16, 24, 32, 48, 64, 96, 128...</returns>
        private int _spriteHeightToLineHeight(int ySize)
        {
            int potSize = 1 << BMath.IntCeilLog2(ySize);
            if (potSize <= 16)
            {
                return BMath.Max(8, potSize);
            }

            if (ySize <= potSize * 0.75f)
            {
                return (int)(potSize * 0.75f);
            }

            return potSize;
        }

        private int _createLine(ushort ySize)
        {
            int freeSpace = _atlasSize.y - _freeYOffset;

            if (freeSpace < ySize)
            {
                throw new InvalidOperationException("Not enough space in atlas to add new sprite");
            }

            SpriteAtlasLine line = new()
            {
                height = ySize,
                sprites = BNative.PermList<ushort>(32),
                freeXOffset = 0,
                yOffset = _freeYOffset
            };

            _freeYOffset += ySize;

            _lines.Add(line);

            return _lines.Count - 1;
        }

        private int _findLine(int requiredWidth, int lineHeight)
        {
            int lineIndex = -1;
            int atlasWidth = _atlasSize.x;
            
            for (int i = 0; i < _lines.Count; i++)
            {
                SpriteAtlasLine line = _lines[i];

                int freeSpace = atlasWidth - line.freeXOffset;
                if (freeSpace < requiredWidth)
                {
                    continue;
                }

                if (line.height == lineHeight)
                {
                    lineIndex = i;
                }    
            }

            return lineIndex;
        }

        public void Dispose()
        {
            foreach (SpriteAtlasLine line in _lines)
            {
                line.Dispose();
            }
            
            _lines.Clear();
            _sprites.Dispose();
        }
    }
}