using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public class DeviceScreen : IScreen
    {
        public int2 Size => new(Screen.width, Screen.height);
        
        public float Aspect => Screen.width / (float)Screen.height;
        
        public int Width => Screen.width;
        
        public int Height => Screen.height;
    }
}