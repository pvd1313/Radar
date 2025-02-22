using System;
using Beast.Claw.Common;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct HalfStream
    {
        public int EntityCount => _entityCount;
        
        private const int HALF_PER_PIXEL = 4;
        private const int BUFFER_CHUNK_SIZE = 128;
        
        private readonly byte _halfPerEntity;
        private readonly bool _usePotResize;
        private readonly Allocator _allocator;
        
        private NativeArray<ushort> _buffer;
        private int _entityCount;
        
        public HalfStream(int pixelPerEntity, Allocator allocator, bool usePotResize)
        {
            if (pixelPerEntity < 0 || pixelPerEntity > 63)
            {
                throw new ArgumentException(nameof(pixelPerEntity));
            }

            _usePotResize = usePotResize;
            _halfPerEntity = (byte) (pixelPerEntity * HALF_PER_PIXEL);
            _entityCount = 0;
            _buffer = default;
            _allocator = allocator;

            _ensureCapacity(BUFFER_CHUNK_SIZE);
        }

        public int2 GetRequiredTextureSize()
        {
            return BGraphic.GetDataTextureSize(_buffer.Length / 4);
        }

        public void CopyToTexture(Texture2D targetTexture)
        {
            targetTexture.SetPixelData(_buffer, 0);
            targetTexture.Apply();
        }

        public NativeSlice<ushort> WriteNextEntity()
        {
            _ensureCapacity(_entityCount + 1);
            
            _entityCount++;

            return _buffer.Slice((_entityCount - 1) * _halfPerEntity, _halfPerEntity);
        }

        public void AddStream(HalfStream source)
        {
            if (_halfPerEntity != source._halfPerEntity)
            {
                throw new InvalidOperationException(
                    "Failed to copy entities from other stream: _halfPerEntity mismatch");
            }
            
            _ensureCapacity(_entityCount + source._entityCount);

            NativeSlice<ushort> targetSlice =
                _buffer.Slice(_entityCount * _halfPerEntity, source._entityCount * _halfPerEntity);

            NativeSlice<ushort> sourceSlice = source._buffer.Slice(0, source._entityCount * _halfPerEntity);
            
            targetSlice.CopyFrom(sourceSlice);
            
            _entityCount += source._entityCount;
        }

        public void AddSlice(NativeSlice<ushort> source)
        {
            if (source.Length % _halfPerEntity != 0)
            {
                throw new InvalidOperationException("Source slice has invalid entity count");
            }

            int sourceEntityCount = source.Length / _halfPerEntity;

            _ensureCapacity(_entityCount + sourceEntityCount);
            
            NativeSlice<ushort> targetSlice =
                _buffer.Slice(_entityCount * _halfPerEntity, sourceEntityCount * _halfPerEntity);
            
            targetSlice.CopyFrom(source);
            
            _entityCount += sourceEntityCount;
        }

        public void Clear()
        {
            _entityCount = 0;
        }

        private void _ensureCapacity(int targetCapacity)
        {
            if (_buffer.IsCreated && _buffer.Length >= targetCapacity * _halfPerEntity)
            {
                return;
            }

            int newSize;

            if (_usePotResize)
            {
                newSize = (1 << BMath.IntCeilLog2(targetCapacity)) * _halfPerEntity;
            }
            else
            {
                newSize = BMath.CeilToInt(targetCapacity / (float) BUFFER_CHUNK_SIZE) * BUFFER_CHUNK_SIZE;
                newSize *= _halfPerEntity;
            }

            NativeArray<ushort> newBuffer = new NativeArray<ushort>(newSize, _allocator);

            if (_buffer.IsCreated)
            {
                newBuffer.CopyFromDifferenceSize(_buffer);
                _buffer.Dispose();
            }
            
            _buffer = newBuffer;
        }

        public void Dispose()
        {
            if (_buffer.IsCreated)
            {
                _buffer.Dispose();
            }
        }
    }
}