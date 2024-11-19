using Beast.Claw.Collections;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class Flow
    {
        public TempArrayStruct<string> TempStringArray { get; } = new(TEMP_ARRAY_CAPACITY);
        public TempArrayStruct<GUIContent> TempContentArray { get; } = new(TEMP_ARRAY_CAPACITY);

        public GUIContent TextNone { get; } = new GUIContent("None");
        
        public GUIStyle CurrentStyle => _currentStyle;
        public GUILayoutOption[] CurrentLayout => _layoutTempArray[_layoutOptionCount];
        
        public void NotifyValidationFailed()
        {
            _validationFailed = true;
        }

        public void NotifyDrawCall()
        {
            if (_drawCallCount >= MAX_DRAW_CALL_COUNT)
            {
                _fatal("Too many draw calls");
            }

            _drawCallCount++;
        }

        public bool TryGetCustomStyle(out GUIStyle style)
        {
            if (_currentStyle != null)
            {
                style = _currentStyle;
                return true;
            }

            style = null;
            return false;
        }
    }
}