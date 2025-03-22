using System;
using System.Runtime.CompilerServices;
using Beast.Claw.Math;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    public struct BezierCurveLengthTable
    {
        public NativeSlice<float> lengthTable;
        
        public static BezierCurveLengthTable Create(
            BezierCurve2D curve, 
            NativeArray<float> buffer, 
            int sampleCount,
            EPrecisionMode mode)
        {
            switch (mode)
            {
                case EPrecisionMode.Float:
                    return _createFloat(curve, buffer, sampleCount);
                
                case EPrecisionMode.Double:
                    return _createDouble(curve, buffer, sampleCount);
                
                default:
                    throw new ArgumentException("Invalid mode");
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BezierCurveLengthTable _createDouble(
            BezierCurve2D curve,
            NativeArray<float> buffer,
            int sampleCount)
        {
            buffer[0] = 0;

            double2 prevPoint = curve.point0;
            double totalLength = 0f;

            for (int i = 1; i <= sampleCount; i++)
            {
                double t = (double)i / sampleCount;
                double2 currentPoint = curve.ComputePointDouble(t);
                totalLength += BMath.Distance(prevPoint, currentPoint);
                buffer[i] = (float)totalLength;
                prevPoint = currentPoint;
            }

            return new BezierCurveLengthTable
            {
                lengthTable = buffer.Slice(0, sampleCount),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BezierCurveLengthTable _createFloat(
            BezierCurve2D curve,
            NativeArray<float> buffer,
            int sampleCount)
        {
            buffer[0] = 0;

            float2 prevPoint = curve.point0;
            float totalLength = 0f;

            for (int i = 1; i <= sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float2 currentPoint = curve.ComputePointFloat(t);
                totalLength += BMath.Distance(prevPoint, currentPoint);
                buffer[i] = totalLength;
                prevPoint = currentPoint;
            }

            return new BezierCurveLengthTable
            {
                lengthTable = buffer.Slice(0, sampleCount),
            };
        }

        public float FindTimeForLength(float targetLength)
        {
            int low = 0;
            int high = lengthTable.Length - 1;

            while (low < high)
            {
                int mid = (low + high) / 2;
                if (lengthTable[mid] < targetLength)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }

            float t1 = (float)(low - 1) / lengthTable.Length;
            float t2 = (float)low / lengthTable.Length;
            float len1 = lengthTable[low - 1];
            float len2 = lengthTable[low];

            float time = t1 + (t2 - t1) * ((targetLength - len1) / (len2 - len1));

            return BMath.Clamp01(time);
        }
    }
}