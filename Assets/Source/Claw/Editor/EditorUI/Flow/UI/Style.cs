using System;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public struct Style
    {
        public bool hasStyle;
        public GUIStyle style;
        
        public static implicit operator Style(GUIStyle guiStyle)
        {
            if (guiStyle == null)
            {
                string message = "Implicitly converted SmartStyle -> GUIStyle. " +
                                 "But style was null. This will result in using default style " +
                                 "Use SmartStyle.NoStyle instead";
                
                BDebug.StaticLogWarning(message);
                
                return default;
            }

            return new Style
            {
                hasStyle = true,
                style = guiStyle
            };
        }

        public float2 CalcSize(Label label)
        {
            if (!hasStyle)
            {
                throw new InvalidOperationException("No style defined");
            }

            if (!label.hasLabel)
            {
                throw new ArgumentException("Label does not have GUIContent");
            }

            return style.CalcSize(label.content);
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