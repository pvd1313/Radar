using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Fancy
{
    internal class GUIStyleScannerWindow : EditorWindow
    {
        private static readonly List<GUIStyle> STYLE_BUFFER = new ();
        
        private readonly SelectableListView<GUIStyle> _listView;
        private readonly RepaintEventsView _repaintEvents;
        
        private List<GUIStyle> _knownStyles;
        private int _selectedIndex;

        public GUIStyleScannerWindow()
        {
            _listView = new SelectableListView<GUIStyle>(s => s.name, GUILayout.Height(300));
            _repaintEvents = new RepaintEventsView(this);
        }

        private void OnGUI()
        {
            _repaintEvents.OnGUI();
            
            if (_knownStyles == null)
            {
                StaticFieldValueScanner.Scan(typeof(EditorWindow).Assembly, STYLE_BUFFER);
                _knownStyles = STYLE_BUFFER.Where(s => !string.IsNullOrWhiteSpace(s.name)).ToList();
            }

            _selectedIndex = _listView.OnGUI(_selectedIndex, _knownStyles);

            EditorGUILayout.Space(10);

            if (_selectedIndex < 0)
            {
                return;
            }

            GUIStyle selectedStyle = _knownStyles[_selectedIndex];
            GUILayout.TextField(selectedStyle.name, selectedStyle);
        }
    }
}