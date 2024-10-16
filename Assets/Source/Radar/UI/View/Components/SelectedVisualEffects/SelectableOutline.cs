using UnityEngine;
using UnityEngine.UI;

namespace Beast.Radar.UI
{
    public class SelectableOutline : SelectableVisual
    {
        [SerializeField] private Outline _outline;
        
        public override void SetDefaultMode()
        {
            _outline.enabled = false;
        }

        public override void SetSelectedMode()
        {
            _outline.enabled = true;
        }
    }
}