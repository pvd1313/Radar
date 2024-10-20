using System;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class Flow
    {
        public void NotifyValidationFailed()
        {
            _validationFailed = true;
        }

        public void NotifyDrawCall()
        {
            if (_drawCallCount >= FATAL_DRAW_CALL_COUNT)
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