using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace Beast.Claw.Input
{
    public struct PackedAxis
    {
        public static PackedAxis None { get; } = default;
        
        private byte _b0;
        private byte _b1;
        private float _f0;
        private float _f1;
        private float _f2;
        private float _f3;
        private NativeSlice<RawTouchAxis> _rawTouch;
        
        // In the future we will probably create NativeBinaryStream,
        // so we will have to forget about this byte boilerplate.
        public PackedAxis(ZoomAxis zoom)
        {
            _b0 = _axisToByte(AxisType.ZOOM);
            _b1 = 0;
            _f0 = zoom.position.x;
            _f1 = zoom.position.y;
            _f2 = zoom.delta;
            _f3 = 0;
            _rawTouch = default;
        }

        public void Unpack(out ZoomAxis zoom)
        {
            _axisUnpackTypeValidation(AxisType.ZOOM);

            zoom = new ZoomAxis
            {
                position = new float2(_f0, _f1),
                delta = _f2
            };
        }

        public PackedAxis(NativeSlice<RawTouchAxis> rawTouch)
        {
            _b0 = _axisToByte(AxisType.RAW_TOUCH);
            _b1 = 0;
            _f0 = 0;
            _f1 = 0;
            _f2 = 0;
            _f3 = 0;
            _rawTouch = rawTouch;
        }

        public void Unpack(out NativeSlice<RawTouchAxis> rawTouch)
        {
            _axisUnpackTypeValidation(AxisType.RAW_TOUCH);

            rawTouch = _rawTouch;
        }

        public PackedAxis(DragAxis drag)
        {
            _b0 = _axisToByte(AxisType.DRAG);
            _b1 = (byte)drag.status;
            _f0 = drag.startPosition.x;
            _f1 = drag.startPosition.y;
            _f2 = drag.position.x;
            _f3 = drag.position.y;
            _rawTouch = default;
        }

        public void Unpack(out DragAxis drag)
        {
            _axisUnpackTypeValidation(AxisType.DRAG);

            drag = new DragAxis
            {
                startPosition = new float2(_f0, _f1),
                position = new float2(_f2, _f3),
                status = (EAxisStatus)_b1
            };
        }

        public AxisType GetAxisType()
        {
            return AxisType.Parse(_b0);
        }

        [Conditional("DEVELOPMENT")]
        private void _axisUnpackTypeValidation(AxisType targetAxis)
        {
            AxisType currentAxis = GetAxisType();
            
            if (targetAxis != currentAxis)
            {
                throw new InvalidOperationException($"Axis was of type {currentAxis}, " +
                                                    $"but was tried to be unpacked as {targetAxis}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte _axisToByte(AxisType axis)
        {
            return (byte)axis.id;
        }
    }
}