using System;
using System.Collections.Generic;
using Beast.Claw.Editor.Fancy.Data;
using Beast.Claw.Editor.Fancy.Tools;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Fancy.Views
{
    internal class TypeScannerWindow
    {
        private const string FilterFocusControlName = "FilterFocusControlName";
        
        private readonly SelectableListView<FancyTypeInfo> _listView;
        private readonly Type _targetType;
        private readonly List<FancyTypeInfo> _filteredTypes;

        private bool _focusedOnce;
        private string _filter;

        public TypeScannerWindow(Type targetType)
        {
            _targetType = targetType;
            _filteredTypes = new List<FancyTypeInfo>();
            _listView = new SelectableListView<FancyTypeInfo>(t => t.nameShort);
            _filter = string.Empty;
        }

        public bool OnGUI(out FancyTypeInfo selectedFancyType)
        {
            selectedFancyType = default;

            GUIStyle searchTextStyle = new (FancyGUIStyles.SearchField);
            GUI.SetNextControlName(FilterFocusControlName);
            _filter = EditorGUILayout.TextField(_filter, searchTextStyle);

            TypeScanner.SearchTypes(_targetType, _filter, _filteredTypes);

            int selectedIndex = _listView.OnGUI(-1, _filteredTypes);
            if (selectedIndex >= 0)
            {
                selectedFancyType = _filteredTypes[selectedIndex];
                return true;
            }

            if (!_focusedOnce)
            {
                _focusedOnce = true;
                GUI.FocusControl(FilterFocusControlName);
            }
            
            return false;
        }
    }
}