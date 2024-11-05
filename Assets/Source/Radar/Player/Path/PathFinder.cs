using System.Diagnostics;
using Beast.Radar.Terrain;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Beast.Radar.Player.Path
{
    public class PathFinder
    {
        private readonly int _size;
        private readonly ByteArray16 _tileSpeedModifiers;
        private readonly NativeArray<EBiomeName> _texture;
        private NativeArray<int2> _neighbourOffsets;

        public PathFinder(int size, ByteArray16 tileSpeedModifiers, NativeArray<EBiomeName> texture)
        {
            _size = size;
            _tileSpeedModifiers = tileSpeedModifiers;
            _texture = texture;

            _neighbourOffsets = new NativeArray<int2>(new[]
            {
                new int2(-1, 0), new int2(1, 0), new int2(0, 1), new int2(0, -1),
                new int2(-1, -1), new int2(-1, 1), new int2(1, -1), new int2(1, 1),
            }, Allocator.Persistent);
        }

        public void Dispose()
        {
            _neighbourOffsets.Dispose();
        }

        public int2[] FindPathArray(int2 startPosition, int2 endPosition, out long elapsedMilliseconds)
        {
            NativeArray<int2> pathNativeArray = FindPath(startPosition, endPosition, out elapsedMilliseconds);
            int2[] result = new int2[pathNativeArray.Length];
            pathNativeArray.CopyTo(result);
            pathNativeArray.Dispose();
            return result;
        }

        private NativeArray<int2> FindPath(int2 startPosition, int2 endPosition, out long elapsedMilliseconds)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            NativeList<int2> pathResultList = new(Allocator.TempJob);

            FindPathJob findPathJob = new()
            {
                textureSizeX = _size,
                startPosition = startPosition,
                endPosition = endPosition,
                outputPath = pathResultList,
                texture = _texture,
                tileSpeedModifiers = _tileSpeedModifiers,
                neighbourOffsets = _neighbourOffsets
            };

            JobHandle jobHandle = findPathJob.Schedule();
            
            jobHandle.Complete();

            stopwatch.Stop();
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            NativeArray<int2> pathNativeArray = new(pathResultList.Length, Allocator.TempJob);

            for (int index = 0; index < pathResultList.Length; index++)
            {
                pathNativeArray[index] = pathResultList[index];
            }

            pathResultList.Dispose();

            return pathNativeArray;
        }
    }
}