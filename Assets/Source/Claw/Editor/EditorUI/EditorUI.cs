using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial struct EditorUI
    {
        public const string CharUpArrow = "↑";
        public const string CharDownArrow = "↓";
        public const string CharPlus = "+";
        public const string CharCross = "x";
        public const string CharA = "A"; 
        public const int SingleCharButtonSize = 20;

        public EditorFlow flow;
        public bool niceLabelsEnabled;
        public GUILayoutOption[] layoutOptions;

        private string NicifyLabel(string label)
        {
            if (!niceLabelsEnabled)
            {
                return label;
            }

            return ObjectNames.NicifyVariableName(label);
        }

        public bool ValidateArgumentNotNull<T>(T obj, string objName) where T : class
        {
            if (obj == null)
            {
                flow.uiErrorsEncountered = true;
                WarningBox($"'{NicifyLabel(objName)}' can not be null");
                return false;
            }

            return true;
        }

        public bool Validate<T>(T obj, string objName) where T : IValidatable
        {
            string errorMessage = obj.Validate();
            
            if (errorMessage != null)
            {
                WarningBox($"'{NicifyLabel(objName)}': {errorMessage}");
                return false;
            }

            return true;
        }

        public bool AssertIsTrue(bool value, string message)
        {
            if (!value)
            {
                flow.uiErrorsEncountered = true;
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                return false;
            }

            return true;
        }
    }
}