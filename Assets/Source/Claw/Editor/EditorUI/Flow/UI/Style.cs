using System;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public struct Style
    {
        public static Style NoStyle { get; } = new(GUIStyle.none, false, true);

        public bool HasStyle => _hasScheme;

        public GUIStyle Scheme
        {
            get
            {
                if (_isReadonly)
                {
                    string message =
                        "This style is readonly. Accessing scheme denied. Use Style.Clone() " +
                        "to create new style with to enable writing.";
                    
                    throw new InvalidOperationException(message);
                }

                return _scheme;
            }
        }
        
        private readonly bool _hasScheme;
        private readonly GUIStyle _scheme;
        private readonly bool _isReadonly;
        
        public Style(GUIStyle style, bool hasStyle, bool isReadonly)
        {
            _scheme = style;
            _hasScheme = hasStyle;
            _isReadonly = isReadonly;
        }

        public static implicit operator GUIStyle(Style style)
        {
            return style._scheme;
        }
        
        public static implicit operator Style(GUIStyle guiStyle)
        {
            if (guiStyle == null)
            {
                string message = "Implicitly converted SmartStyle -> GUIStyle. " +
                                 "But style was null. This will result in using default style " +
                                 "Use SmartStyle.NoStyle instead";
                
                BDebug.StaticLogWarning(message);
                
                return NoStyle;
            }

            return new Style(new GUIStyle(guiStyle), true, true);
        }

        public Style Clone()
        {
            ValidateHasStyleThrowException();

            return new Style(new GUIStyle(_scheme), true, false);
        }

        public float2 CalcSize(Label label)
        {
            ValidateHasStyleThrowException();

            if (!label.hasLabel)
            {
                throw new ArgumentException("Label does not have GUIContent");
            }

            return _scheme.CalcSize(label.content);
        }

        public void ValidateHasStyleThrowException()
        {
            if (!_hasScheme)
            {
                throw new ValidationException("style must be set");
            }
        }
    }
}