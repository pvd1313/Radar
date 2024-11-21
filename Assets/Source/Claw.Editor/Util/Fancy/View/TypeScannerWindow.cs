using System.Collections.Generic;
using Beast.Claw.Reflection;
using Beast.Claw.Text;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Fancy
{
    internal class TypeScannerWindow
    {
        private const string FILTER_FOCUS_CONTROL = "f258c81-c73e-47d8-9df1-9cd35b4f9513";
        
        private readonly TypeScanner _scanner;
        private readonly SelectableListView<TypeData> _listView;
        private readonly StringWithFormat _filterTextLowerCase;
        
        private bool _focusedOnce;
        private string _filterText;

        public TypeScannerWindow(TypeScanner scanner)
        {
            _scanner = scanner;
            _listView = new SelectableListView<TypeData>(t => t.nameShort);
            _filterText = string.Empty;
            _filterTextLowerCase = new StringWithFormat(_filterText, EStringFormat.LowerCase);
        }

        public bool OnGUI(out TypeData selectedFancyType)
        {
            selectedFancyType = default;

            GUIStyle searchTextStyle = new (FancyGUIStyles.SEARCH_FIELD);
            GUI.SetNextControlName(FILTER_FOCUS_CONTROL);
            _filterText = EditorGUILayout.TextField(_filterText, searchTextStyle);

            _filterTextLowerCase.Update(_filterText);

            IReadOnlyList<TypeData> types = _scanner.Filter(_filterTextLowerCase);

            int selectedIndex = _listView.OnGUI(-1, types);
            
            if (selectedIndex >= 0)
            {
                selectedFancyType = types[selectedIndex];
                return true;
            }

            if (!_focusedOnce)
            {
                _focusedOnce = true;
                
                GUI.FocusControl(FILTER_FOCUS_CONTROL);
            }
            
            return false;
        }
    }
}