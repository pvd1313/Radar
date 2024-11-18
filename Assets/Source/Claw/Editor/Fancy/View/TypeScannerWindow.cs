using System;
using System.Collections.Generic;
using Beast.Claw.Text;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Fancy
{
    internal class TypeScannerWindow
    {
        private const string FILTER_FOCUS_CONTROL_NAME = "FilterFocusControlName";
        
        private readonly SelectableListView<FancyTypeInfo> _listView;
        private readonly Type[] _targetTypes;
        private readonly Type[] _excludeTypes;
        private readonly List<FancyTypeInfo> _filteredTypes;
        private readonly StringWithFormat _filterLowerCase;
        
        private bool _focusedOnce;
        private string _filter;

        public TypeScannerWindow(Type[] targetTypes, params Type[] excludeTypes)
        {
            _targetTypes = targetTypes;
            _filteredTypes = new List<FancyTypeInfo>();
            _excludeTypes = excludeTypes;
            _listView = new SelectableListView<FancyTypeInfo>(t => t.nameShort);
            _filter = string.Empty;
            _filterLowerCase = new StringWithFormat(_filter, EStringFormat.LowerCase);
        }

        public bool OnGUI(out FancyTypeInfo selectedFancyType)
        {
            selectedFancyType = default;

            GUIStyle searchTextStyle = new (FancyGUIStyles.SEARCH_FIELD);
            GUI.SetNextControlName(FILTER_FOCUS_CONTROL_NAME);
            _filter = EditorGUILayout.TextField(_filter, searchTextStyle);

            _filterLowerCase.Update(_filter);

            TypeScanner.SearchTypes(_targetTypes, _filterLowerCase, _filteredTypes, _excludeTypes);

            int selectedIndex = _listView.OnGUI(-1, _filteredTypes);
            if (selectedIndex >= 0)
            {
                selectedFancyType = _filteredTypes[selectedIndex];
                return true;
            }

            if (!_focusedOnce)
            {
                _focusedOnce = true;
                GUI.FocusControl(FILTER_FOCUS_CONTROL_NAME);
            }
            
            return false;
        }
    }
}