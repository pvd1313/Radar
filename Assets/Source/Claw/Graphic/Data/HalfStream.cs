using System;
using Beast.Claw.Common;
using Beast.Claw.Jobs;
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
        private readonly byte _pixelPerEntity;
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
            _pixelPerEntity =  (byte) pixelPerEntity;
            _halfPerEntity = (byte) (_pixelPerEntity * HALF_PER_PIXEL);
            _entityCount = 0;
            _buffer = default;
            _allocator = allocator;

            _ensureCapacity(BUFFER_CHUNK_SIZE);
        }

        public int2 GetRequiredTextureSize()
        {
            return BGraphic.GetDataTextureSize(_entityCount * _pixelPerEntity);
        }

        public void CopyToTexture(Texture2D targetTexture)
        {
            int dataSize = targetTexture.width * targetTexture.height * _halfPerEntity;

            if (dataSize == _buffer.Length)
            {
                targetTexture.SetPixelData(_buffer, 0);
            }
            else
            {
                NativeArray<ushort> tempBuffer = BNative.TempArray<ushort>(dataSize);

                int minLength = BMath.Min(tempBuffer.Length, _buffer.Length);

                NativeSlice<ushort> source = _buffer.Slice(0, minLength);
                NativeSlice<ushort> target = tempBuffer.Slice(0, minLength);
                target.CopyFrom(source);
                
                targetTexture.SetPixelData(tempBuffer, 0);

                tempBuffer.Dispose();
            }
            
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
                newSize = _getPotSize(targetCapacity);
            }
            else
            {
                newSize = BMath.CeilToInt(targetCapacity / (float) BUFFER_CHUNK_SIZE) * BUFFER_CHUNK_SIZE;
                newSize *= _halfPerEntity;
            }

            NativeArray<ushort> newBuffer = new (newSize, _allocator);

            if (_buffer.IsCreated)
            {
                newBuffer.CopyFromDifferenceSize(_buffer);
                _buffer.Dispose();
            }
            
            _buffer = newBuffer;
        }

        private int _getPotSize(int entityCount)
        {
            return (1 << BMath.IntCeilLog2(entityCount)) * _halfPerEntity;
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