using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public struct Label
    {
        public static Label NoLabel { get; } = default;
        
        public bool hasLabel;
        public GUIContent content;
        
        public static implicit operator Label(GUIContent guiContent)
        {
            if (guiContent == null)
            {
                string message = "Implicitly converted SmartLabel -> GUIContent. " +
                                 "But content was null. This will result in empty label. " +
                                 "Use Label.NoLabel instead";
                
                BDebug.StaticLogWarning(message);
                
                return NoLabel;
            }

            return new Label
            {
                hasLabel = true,
                content = guiContent
            };
        }

        public static implicit operator Label(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                string message = "Implicitly converted string -> SmartLabel. " +
                                 "But text was null or empty. This will result in empty label. " +
                                 "Use Label.NoLabel instead";
                
                BDebug.StaticLogWarning(message);

                return NoLabel;
            }

            return new Label
            {
                hasLabel = true,
                content = EditorGUIUtility.TrTempContent(text)
            };
        }

        public void ValidateHasLabelThrowException()
        {
            if (!hasLabel)
            {
                throw new ValidationException("label must be set");
            }
        }
    }
}