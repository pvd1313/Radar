using System;
using Unity.Mathematics;

namespace Beast.Claw.Collections
{
    /// <summary>
    /// Returns 8 delta positions for quad points and middle centers. Counter - clockwise order.
    /// </summary>
    public struct QuadPointIterator
    {
        private int _index;
        private int _radius;
        private int _minusRadius;
        
        public QuadPointIterator(int radius)
        {
            if (radius < 0)
            {
                throw new ArgumentException(nameof(radius));
            }

            _radius = radius;
            _minusRadius = _radius * -1;
            _index = 0;
        }

        public bool GotNext(out int2 delta)
        {
            delta = int2.zero;
            
            if (_radius == 0)
            {
                if (_index == 0)
                {
                    _index++;
                    return true;
                }

                return false;
            }

            switch (_index)
            {
                // 0 0 0
                // 0   0
                // 1 0 0
                case 0:
                    delta = new int2(_minusRadius, _minusRadius);
                    break;
                
                // 0 0 0
                // 0   0
                // 0 1 0
                case 1:
                    delta = new int2(0, _minusRadius);
                    break;
                
                // 0 0 0
                // 0   0
                // 0 0 1
                case 2:
                    delta = new int2(_radius, _minusRadius);
                    break;
                
                // 0 0 0
                // 0   1
                // 0 0 0
                case 3:
                    delta = new int2(_radius, 0);
                    break;
                
                // 0 0 1
                // 0   0
                // 0 0 0
                case 4:
                    delta = new int2(_radius, _radius);
                    break;
                
                // 0 1 0
                // 0   0
                // 0 0 0
                case 5:
                    delta = new int2(0, _radius);
                    break;
                
                // 1 0 0
                // 0   0
                // 0 0 0
                case 6:
                    delta = new int2(_minusRadius, _radius);
                    break;
                
                // 0 0 0
                // 1   0
                // 0 0 0
                case 7:
                    delta = new int2(_minusRadius, 0);
                    break;
                
                // Finished.
                default:
                    return false;
            }

            _index++;

            return true;
        }
    }
}