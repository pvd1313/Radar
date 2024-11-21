using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public readonly ref struct RectSlice
    {
        private readonly Span<Rect> _span;

        public RectSlice(Span<Rect> span)
        {
            _span = span;
        }

        public void Columns(Rect rect, float w0, float w1)
        {
            _validateSpanCapacity(2);

            float offset = 0;
            _span[0] = _widthSlice(rect, ref offset, w0);
            _span[1] = _widthSlice(rect, ref offset, w1);
        }
        
        public void Columns(Rect rect, float w0, float w1, float w2)
        {
            _validateSpanCapacity(3);
            
            float offset = 0;
            _span[0] = _widthSlice(rect, ref offset, w0);
            _span[1] = _widthSlice(rect, ref offset, w1);
            _span[2] = _widthSlice(rect, ref offset, w2);
        }
        
        public void Columns(Rect rect, float w0, float w1, float w2, float w3)
        {
            _validateSpanCapacity(4);
            
            float offset = 0;
            _span[0] = _widthSlice(rect, ref offset, w0);
            _span[1] = _widthSlice(rect, ref offset, w1);
            _span[2] = _widthSlice(rect, ref offset, w2);
            _span[3] = _widthSlice(rect, ref offset, w3);
        }
        
        public void Columns(Rect rect, float w0, float w1, float w2, float w3, float w4)
        {
            _validateSpanCapacity(5);
            
            float offset = 0;
            _span[0] = _widthSlice(rect, ref offset, w0);
            _span[1] = _widthSlice(rect, ref offset, w1);
            _span[2] = _widthSlice(rect, ref offset, w2);
            _span[3] = _widthSlice(rect, ref offset, w3);
            _span[4] = _widthSlice(rect, ref offset, w4);
        }
        
        public void Columns(Rect rect, float w0, float w1, float w2, float w3, float w4, float w5)
        {
            _validateSpanCapacity(6);
            
            float offset = 0;
            _span[0] = _widthSlice(rect, ref offset, w0);
            _span[1] = _widthSlice(rect, ref offset, w1);
            _span[2] = _widthSlice(rect, ref offset, w2);
            _span[3] = _widthSlice(rect, ref offset, w3);
            _span[4] = _widthSlice(rect, ref offset, w4);
            _span[5] = _widthSlice(rect, ref offset, w5);
        }
        
        public void Columns(Rect rect, float w0, float w1, float w2, float w3, float w4, float w5, float w6)
        {
            _validateSpanCapacity(7);
            
            float offset = 0;
            _span[0] = _widthSlice(rect, ref offset, w0);
            _span[1] = _widthSlice(rect, ref offset, w1);
            _span[2] = _widthSlice(rect, ref offset, w2);
            _span[3] = _widthSlice(rect, ref offset, w3);
            _span[4] = _widthSlice(rect, ref offset, w4);
            _span[5] = _widthSlice(rect, ref offset, w5);
            _span[6] = _widthSlice(rect, ref offset, w6);
        }
        
        public void Columns(Rect rect, float w0, float w1, float w2, float w3, float w4, float w5, float w6, float w7)
        {
            _validateSpanCapacity(8);
            
            float offset = 0;
            _span[0] = _widthSlice(rect, ref offset, w0);
            _span[1] = _widthSlice(rect, ref offset, w1);
            _span[2] = _widthSlice(rect, ref offset, w2);
            _span[3] = _widthSlice(rect, ref offset, w3);
            _span[4] = _widthSlice(rect, ref offset, w4);
            _span[5] = _widthSlice(rect, ref offset, w5);
            _span[6] = _widthSlice(rect, ref offset, w6);
            _span[7] = _widthSlice(rect, ref offset, w7);
        }

        public void Rows(Rect rect, float h0, float h1)
        {
            _validateSpanCapacity(2);

            float offset = 0;
            _span[0] = _heightSlice(rect, ref offset, h0);
            _span[1] = _heightSlice(rect, ref offset, h1);
        }
        
        public void Rows(Rect rect, float h0, float h1, float h2)
        {
            _validateSpanCapacity(3);

            float offset = 0;
            _span[0] = _heightSlice(rect, ref offset, h0);
            _span[1] = _heightSlice(rect, ref offset, h1);
            _span[2] = _heightSlice(rect, ref offset, h2);
        }
        
        public void Rows(Rect rect, float h0, float h1, float h2, float h3)
        {
            _validateSpanCapacity(4);

            float offset = 0;
            _span[0] = _heightSlice(rect, ref offset, h0);
            _span[1] = _heightSlice(rect, ref offset, h1);
            _span[2] = _heightSlice(rect, ref offset, h2);
            _span[3] = _heightSlice(rect, ref offset, h3);
        }
        
        public void Rows(Rect rect, float h0, float h1, float h2, float h3, float h4)
        {
            _validateSpanCapacity(5);

            float offset = 0;
            _span[0] = _heightSlice(rect, ref offset, h0);
            _span[1] = _heightSlice(rect, ref offset, h1);
            _span[2] = _heightSlice(rect, ref offset, h2);
            _span[3] = _heightSlice(rect, ref offset, h3);
            _span[4] = _heightSlice(rect, ref offset, h4);
        }
        
        public void Rows(Rect rect, float h0, float h1, float h2, float h3, float h4, float h5)
        {
            _validateSpanCapacity(6);

            float offset = 0;
            _span[0] = _heightSlice(rect, ref offset, h0);
            _span[1] = _heightSlice(rect, ref offset, h1);
            _span[2] = _heightSlice(rect, ref offset, h2);
            _span[3] = _heightSlice(rect, ref offset, h3);
            _span[4] = _heightSlice(rect, ref offset, h4);
            _span[5] = _heightSlice(rect, ref offset, h5);
        }
        
        public void Rows(Rect rect, float h0, float h1, float h2, float h3, float h4, float h5, float h6)
        {
            _validateSpanCapacity(7);

            float offset = 0;
            _span[0] = _heightSlice(rect, ref offset, h0);
            _span[1] = _heightSlice(rect, ref offset, h1);
            _span[2] = _heightSlice(rect, ref offset, h2);
            _span[3] = _heightSlice(rect, ref offset, h3);
            _span[4] = _heightSlice(rect, ref offset, h4);
            _span[5] = _heightSlice(rect, ref offset, h5);
            _span[6] = _heightSlice(rect, ref offset, h6);
        }
        
        public void Rows(Rect rect, float h0, float h1, float h2, float h3, float h4, float h5, float h6, float h7)
        {
            _validateSpanCapacity(8);

            float offset = 0;
            _span[0] = _heightSlice(rect, ref offset, h0);
            _span[1] = _heightSlice(rect, ref offset, h1);
            _span[2] = _heightSlice(rect, ref offset, h2);
            _span[3] = _heightSlice(rect, ref offset, h3);
            _span[4] = _heightSlice(rect, ref offset, h4);
            _span[5] = _heightSlice(rect, ref offset, h5);
            _span[6] = _heightSlice(rect, ref offset, h6);
            _span[7] = _heightSlice(rect, ref offset, h7);
        }
        
        public Rect this[int index]
        {
            get
            {
                if (index < 0 || index > _span.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return _span[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Rect _widthSlice(Rect screen, ref float offset, float width)
        {
            screen = new Rect(screen.x + offset, screen.y, width, screen.height);

            offset += width;

            return screen;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Rect _heightSlice(Rect screen, ref float offset, float height)
        {
            screen = new Rect(screen.x, screen.y + offset, screen.width, height);

            offset += height;

            return screen;
        }

        private void _validateSpanCapacity(int requiredCapacity)
        {
            if (requiredCapacity > _span.Length)
            {
                throw new InvalidOperationException("Stack does not have enough capacity: " + requiredCapacity);
            }
        }
    }
}