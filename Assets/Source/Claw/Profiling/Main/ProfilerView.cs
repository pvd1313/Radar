using System.Collections.Generic;
using UnityEngine;

namespace Beast.Claw.Profiling
{
    public class ProfilerView
    {
        private readonly FloatToStringConverter _converter = new();

        public void OnGUI(Rect rect, IEnumerable<ProfilerNode> nodes)
        {
            rect.height = 20;
            Rect rectLeft = new (rect.width - 32, rect.y, 20, 20);
            Rect rectRight = new (rect.width - 19, rect.y, 25, 20);
            
            foreach (ProfilerNode node in nodes)
            {
                rect.x = 20 * node.deep + 3;

                GUI.Label(rect, node.name);
                
                _converter.ToString(node.time, out string exponent, out string fraction);
                
                if ((int) node.time > 0)
                {
                    GUI.Label(rectLeft, exponent);
                }
                
                GUI.Label(rectRight, fraction);

                rect.y += 20;
                rectRight.y += 20;
                rectLeft.y += 20;
            }
        }
    }
}