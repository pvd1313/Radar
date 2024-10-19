using UnityEngine;

namespace Beast.Claw.Editor
{
    partial class EditorFlow
    {
        public void NotifyValidationFailed()
        {
            _validationFailed = true;
        }

        public bool TryGetCustomStyle(out GUIStyle style)
        {
            if (_customStyle != null)
            {
                style = _customStyle;
                return true;
            }

            style = null;
            return false;
        }
    }
}