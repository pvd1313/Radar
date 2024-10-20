using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public struct Style
    {
        public static Style NoStyle { get; } = default;
        
        public bool hasStyle;
        public GUIStyle style;
        
        public static implicit operator Style(GUIStyle guiStyle)
        {
            if (guiStyle == null)
            {
                string message = "Implicitly converted SmartStyle -> GUIStyle. " +
                                 "But style was null. This will result in default style " +
                                 "Use SmartStyle.NoStyle instead";
                
                BDebug.StaticLogWarning(message);
                
                return NoStyle;
            }

            return new Style
            {
                hasStyle = true,
                style = guiStyle
            };
        }

        public void ValidateHasStyleThrowException()
        {
            if (!hasStyle)
            {
                throw new ValidationException("style must be set");
            }
        }
    }
}