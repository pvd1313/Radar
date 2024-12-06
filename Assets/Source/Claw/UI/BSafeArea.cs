using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.UI
{
    public static class BSafeArea
    {
        public struct Padding
        {
            public float left;
            public float right;
            public float down;
            public float up;
        }

        private static Padding _padding;
        private static Rect _safeArea;

        public static Padding GetPadding()
        {
            Rect safeArea = Screen.safeArea;

            if (!safeArea.Equals(_safeArea))
            {
                _safeArea = safeArea;

                int2 screenSize = new(Screen.width, Screen.height);

                _padding = new Padding
                {
                    left = BMath.Max(0, safeArea.x / screenSize.x),
                    right = BMath.Max(0, (screenSize.x - safeArea.x - safeArea.width) / screenSize.x),
                    down = BMath.Max(0, safeArea.y / screenSize.y),
                    up = BMath.Max(0, (screenSize.y - safeArea.y - safeArea.height) / screenSize.y)
                };
                
                return _padding;
            }

            return _padding;
        }
    }
}