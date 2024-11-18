using Beast.Claw.Common;
using Beast.Claw.Validation;
using UnityEditor;

namespace Beast.Claw.Editor
{
    public readonly partial struct FlowUI
    {
        public bool ValidateArgumentNotNull<T>(T obj, string objName) where T : class
        {
            if (obj == null)
            {
                _flow.NotifyValidationFailed();
                WarningBox($"'{objName}' can not be null");
                return false;
            }

            return true;
        }
        
        public bool Validate<T>(T obj, string objName) where T : IValidatable
        {
            string errorMessage = obj.Validate();

            if (errorMessage != null)
            {
                _flow.NotifyValidationFailed();
                WarningBox($"'{objName}': {errorMessage}");
                return false;
            }

            return true;
        }

        public bool ValidateIsTrue(bool value, string message)
        {
            if (!value)
            {
                _flow.NotifyValidationFailed();
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                return false;
            }

            return true;
        }
    }
}