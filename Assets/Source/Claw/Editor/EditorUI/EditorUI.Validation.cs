using UnityEditor;

namespace Beast.Claw.Editor
{
    public partial struct EditorUI
    {
        public bool ValidateArgumentNotNull<T>(T obj, string objName) where T : class
        {
            if (obj == null)
            {
                flow.NotifyValidationFailed();
                WarningBox($"'{flow.NicifyLabel(objName)}' can not be null");
                return false;
            }

            return true;
        }
    }

    public partial struct EditorUI
    {
        public bool Validate<T>(T obj, string objName) where T : IValidatable
        {
            string errorMessage = obj.Validate();

            if (errorMessage != null)
            {
                flow.NotifyValidationFailed();
                WarningBox($"'{flow.NicifyLabel(objName)}': {errorMessage}");
                return false;
            }

            return true;
        }

        public bool ValidateIsTrue(bool value, string message)
        {
            if (!value)
            {
                flow.NotifyValidationFailed();
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                return false;
            }

            return true;
        }
    }
}