using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Radar.UI.Panels.Base
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BView : BeastMono
    {
        public RectTransform rect;
        public RectTransform body;

        private void OnValidate()
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }
        }
    }
}