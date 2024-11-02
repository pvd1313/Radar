using System;
using Unity.Mathematics;

namespace Beast.Radar.City
{
    /// <summary>
    /// Returns delta positions with given radius for a quad perimeter. Counter - clockwise order.
    /// </summary>
    public struct QuadPerimeterIterator
    {
        private enum EMode
        {
            BottomLine,
            RightLine,
            TopLine,
            LeftLine,
            Finished
        }

        private readonly int _radius;

        private int _index;
        private EMode _mode;

        public QuadPerimeterIterator(int radius)
        {
            if (radius < 0)
            {
                throw new ArgumentException(nameof(radius));
            }

            _index = 0;
            _radius = radius;
            _mode = EMode.BottomLine;
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

            int deltaX, deltaY;

            switch (_mode)
            {
                case EMode.BottomLine:
                    deltaX = _index - _radius;
                    delta = new int2(deltaX, _radius * -1);

                    if (deltaX == _radius)
                    {
                        _mode = EMode.RightLine;
                        _index = 0;
                    }
                    else
                    {
                        _index++;
                    }

                    return true;

                case EMode.RightLine:
                    deltaY = _index + 1 - _radius;
                    delta = new int2(_radius, deltaY);

                    if (deltaY == _radius)
                    {
                        _mode = EMode.TopLine;
                        _index = 0;
                    }
                    else
                    {
                        _index++;
                    }

                    return true;

                case EMode.TopLine:
                    deltaX = _radius - _index - 1;
                    delta = new int2(deltaX, _radius);

                    if (deltaX == -_radius)
                    {
                        _mode = EMode.LeftLine;
                        _index = 0;
                    }
                    else
                    {
                        _index++;
                    }

                    return true;

                case EMode.LeftLine:
                    deltaY = _radius - _index - 1;
                    delta = new int2(_radius * -1, deltaY);

                    if (deltaY == 1 - _radius)
                    {
                        _mode = EMode.Finished;
                    }
                    else
                    {
                        _index++;
                    }

                    return true;

                default:
                    return false;
            }
        }
    }
}