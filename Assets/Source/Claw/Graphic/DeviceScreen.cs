using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public class DeviceScreen : IScreen
    {
        public int2 WidthHeight => new(Screen.width, Screen.height);
        public float Aspect => Screen.width / (float)Screen.height;
        
        public float Width => Screen.width;
        
        public float Height => Screen.height;
    }
}