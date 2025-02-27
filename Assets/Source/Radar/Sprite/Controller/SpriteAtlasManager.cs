using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Graphic;
using Beast.Claw.Jobs;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

namespace Beast.Radar.Sprite
{
    public class SpriteAtlasManager
    {
        public NativeSlice<SpriteInfo> Sprites => _sprites.AsArray().Slice();
        public RenderTexture AtlasTexture => _atlasTexture;

        private readonly List<SpriteAtlasLine> _lines;
        private readonly List<ushort> _freeSpriteIndexList;
        private readonly HashSet<ushort> _linesWithGaps;

        private RenderTexture _atlasTexture;
        private RenderTexture _bufferTexture;
        private NativeList<SpriteInfo> _sprites;
        private ushort _freeYOffset;
        
        public SpriteAtlasManager(IController owner, RenderTextureData atlasParams)
        {
            _atlasTexture = BGraphic.CreatePermRenderTexture(owner, atlasParams);

            BGraphic.ClearTexture(_atlasTexture);

            atlasParams.size = 256;
            atlasParams.name = "Buffer";
            _bufferTexture = BGraphic.CreatePermRenderTexture(owner, atlasParams);
            
            _lines = new List<SpriteAtlasLine>(64);
            _freeSpriteIndexList = new List<ushort>(128);
            _linesWithGaps = new HashSet<ushort>(64);
            
            _sprites = BNative.PermList<SpriteInfo>(256);
            _sprites.Add(default);
        }

        public ushort Create(SpriteSetup setup)
        {
            Profiler.BeginSample("SpriteAtlasManager.Create()");
            
            int lineHeight = _spriteHeightToLineHeight(setup.textureSize.y);

            int lineIndex = _findLine(setup.textureSize.x, lineHeight);
            
            if (lineIndex == -1)
            {
                lineIndex = _createLine((ushort) lineHeight);
            }

            int lastIndex = _freeSpriteIndexList.Count - 1;
            ushort spriteIndex;

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

            _sprites[spriteIndex] = new SpriteInfo
            {
                atlasOffset = new short2(line.freeXOffset, line.yOffset),
                size = (short2)setup.textureSize,
                lineIndex = (byte) lineIndex
            };
            
            BGraphic.DrawTextureToRenderTexture(new TextureToRenderTextureData
            {
                sourceTexture = setup.sourceTexture,
                sourceOffset = setup.textureOffset,
                sourceSize = setup.textureSize,
                targetTexture = _atlasTexture,
                targetOffset = new short2(line.freeXOffset, line.yOffset)
            });

            line.freeXOffset = (ushort)(line.freeXOffset + setup.textureSize.x);
            _lines[lineIndex] = line;
            
            Profiler.EndSample();

            return spriteIndex;
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

        public void PackGaps()
        {
            foreach (ushort lineIndex in _linesWithGaps)
            {
                SpriteAtlasLine line = _lines[lineIndex];
        
                int xOffset = 0;
        
                for (int i = 0; i < line.sprites.Length; i++)
                {
                    ushort spriteID = line.sprites[i];
                    SpriteInfo sprite = _sprites[spriteID];
                    
                    // Has gap.
                    if (xOffset < sprite.atlasOffset.x)
                    {
                        BGraphic.ClearTexture(_bufferTexture);
                        
                        BGraphic.DrawTextureToRenderTexture(new TextureToRenderTextureData
                        {
                            sourceTexture = _atlasTexture,
                            targetTexture = _bufferTexture,
                            sourceSize = sprite.size,
                            sourceOffset = sprite.atlasOffset,
                        });
                        
                        BGraphic.DrawTextureToRenderTexture(new TextureToRenderTextureData
                        {
                            sourceTexture = _bufferTexture,
                            targetTexture = _atlasTexture,
                            sourceSize = sprite.size,
                            targetOffset = new int2(xOffset, sprite.atlasOffset.y)
                        });
        
                        sprite.atlasOffset.x = (short)xOffset;
        
                        _sprites[spriteID] = sprite;
                    }
        
                    xOffset += sprite.size.x;
                }
        
                line.freeXOffset = (ushort)xOffset;
        
                _lines[lineIndex] = line;
            }
            
            _linesWithGaps.Clear();
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
            int freeSpace = _atlasTexture.height - _freeYOffset;

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
            int atlasWidth = _atlasTexture.width;
            
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
            
            BGraphic.TryDispose(ref _atlasTexture);
            BGraphic.TryDispose(ref _bufferTexture);
        }
    }
}