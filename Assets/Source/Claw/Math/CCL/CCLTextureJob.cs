using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Claw.Math.CCL
{
    /// <summary>
    /// Connected component labeling. 8 connections on 2D texture. Allows to recognize separate shapes.
    /// </summary>
    [BurstCompile]
    public struct CCLTextureJob<TTexture> : IJob 
        where TTexture : ICCLTexture
    {
        public TTexture inTexture;
        public NativeArray<ushort> outLabeledTexture;

        [NativeDisableContainerSafetyRestriction] private NativeHashSet<int> _labelEquivalency;
        [NativeDisableContainerSafetyRestriction] private NativeTexture<ushort> _textureWithLabels;
        [NativeDisableContainerSafetyRestriction] private ushort _lastLabel;
        [NativeDisableContainerSafetyRestriction] private NativeDisjointSet _labelUnion;
        [NativeDisableContainerSafetyRestriction] private NativeArray<ushort> _labelUnionBaked;

        public void Execute()
        {
            int2 inTextureSize = inTexture.Size;
            
            if (outLabeledTexture.Length != inTextureSize.x * inTextureSize.y)
            {
                throw new InvalidOperationException(
                    $"Output texture size ({outLabeledTexture.Length}) does not match input texture ({inTextureSize})");
            }

            _labelEquivalency = new NativeHashSet<int>(1024, Allocator.Temp);
            _textureWithLabels = new NativeTexture<ushort>(outLabeledTexture, inTextureSize);

            _textureLabelingFirstPass();

            _labelUnion = new NativeDisjointSet(_lastLabel, Allocator.Temp);

            foreach (int value in _labelEquivalency)
            {
                int min = value & 0xFFFF;
                int max = value >> 16;

                _labelUnion.Union(min, max);
            }

            _labelUnionBaked = new NativeArray<ushort>(_lastLabel + 1, Allocator.Temp);
            for (int i = 1; i <= _lastLabel; i++)
            {
                ushort value = (ushort)_labelUnion.Find(i);
                _labelUnionBaked[i] = value;
            }
            
            _textureLabelingSecondPass();

            _labelUnion.Dispose();
            _labelEquivalency.Dispose();
        }

        /// <summary>
        /// Replace all labels with root labels.
        /// </summary>
        private void _textureLabelingSecondPass()
        {
            int textureSize = inTexture.Size.x * inTexture.Size.y;

            ushort prevLabel = ushort.MaxValue;
            ushort prevLabelRoot = ushort.MaxValue;
            
            for (int i = 0; i < textureSize; i++)
            {
                ushort label = outLabeledTexture[i];
                if (label == 0)
                {
                    continue;
                }

                if (label != prevLabel)
                {
                    prevLabel = label;
                    prevLabelRoot = _labelUnionBaked[label];
                }

                outLabeledTexture[i] = prevLabelRoot;
            }
        }
        
        /// <summary>
        /// Use connected component link to fill label texture, with 8 connection in mind.
        /// </summary>
        private void _textureLabelingFirstPass()
        {
            int2 textureSize = inTexture.Size;
            
            for (int y = 0; y < textureSize.y - 1; y++)
            for (int x = 0; x < textureSize.x - 1; x++)
            {
                if (!inTexture.TestPixel(x, y))
                {
                    continue;
                }

                ushort leftLabel = _getLabelOrDefault(x - 1, y);

                if (leftLabel > 0)
                {
                    ushort bottomRightLabel = _getLabelOrDefault(x + 1, y - 1);

                    if (bottomRightLabel == 0 || bottomRightLabel == leftLabel)
                    {
                        _textureWithLabels.SetPixel(x, y, leftLabel);
                        continue;
                    }

                    BMath.MinMax(leftLabel, bottomRightLabel, out int min, out int max);
                    _textureWithLabels.SetPixel(x, y, (ushort)min);
                    _addEquivalency(min, max);
                    continue;
                }

                ushort bottomLabel = _getLabelOrDefault(x, y - 1);
                if (bottomLabel > 0)
                {
                    _textureWithLabels.SetPixel(x, y, bottomLabel);
                    continue;
                }

                ushort bottomLeftLabel = _getLabelOrDefault(x - 1, y - 1);
                ushort bottomRightLabel1 = _getLabelOrDefault(x + 1, y - 1);
                if (bottomLeftLabel > 0 || bottomRightLabel1 > 0)
                {
                    if (bottomLeftLabel > 0 && bottomRightLabel1 > 0)
                    {
                        if (bottomLeftLabel == bottomRightLabel1)
                        {
                            _textureWithLabels.SetPixel(x, y, bottomLeftLabel);
                            continue;
                        }
                        
                        BMath.MinMax(bottomLeftLabel, bottomRightLabel1, out int min, out int max);
                        _textureWithLabels.SetPixel(x, y, (ushort)min);
                        _addEquivalency(min, max);
                    }

                    if (bottomLeftLabel > 0)
                    {
                        _textureWithLabels.SetPixel(x, y, bottomLeftLabel);
                        continue;
                    }
                    
                    _textureWithLabels.SetPixel(x, y, bottomRightLabel1);
                    continue;
                }

                _lastLabel++;
                _textureWithLabels.SetPixel(x, y, _lastLabel);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _addEquivalency(int min, int max)
        {
            _labelEquivalency.Add((max << 16) | min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ushort _getLabelOrDefault(int x, int y)
        {
            if (x < 0 || y < 0 || x >= inTexture.Size.x || y >= inTexture.Size.y)
            {
                return 0;
            }

            return _textureWithLabels.GetPixel(x, y);
        }
    }
}