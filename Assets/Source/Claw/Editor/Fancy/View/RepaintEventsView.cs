using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Fancy
{
    internal class RepaintEventsView
    {
        private readonly EditorWindow _owner;

        public RepaintEventsView(EditorWindow owner)
        {
            _owner = owner;
        }
        
        public void OnGUI()
        {
            Event curEvent = Event.current;
            
            if (curEvent is { type: EventType.KeyDown })
            {
                _owner.Repaint();
            }
        }
    }
}