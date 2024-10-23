using System;
using Beast.Claw.Collection;
using Beast.Claw.Debug;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public struct Label
    {
        public static Label NoLabel => new(GUIContent.none, false);

        private static ObjectPoolStructType<GUIContent> _pool;
        
        public bool hasLabel;
        public GUIContent content;

        static Label()
        {
            try
            {
                _pool = new ObjectPoolStructType<GUIContent>(32);
            }
            catch (Exception e)
            {
                BDebug.StaticLogException(e);
            }
        }

        public Label(GUIContent content, bool hasLabel)
        {
            this.content = content;
            this.hasLabel = hasLabel;
        }
        
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

            GUIContent content = _pool.Next();
            content.text = text;

            return new Label
            {
                hasLabel = true,
                content = content
            };
        }

        public float2 CalcSizeOrDefault(Style style)
        {
            if (!hasLabel)
            {
                return 0;
            }

            if (!style.HasStyle)
            {
                throw new ArgumentException("Style not set");
            }
            
            return style.CalcSize(content);
        }
        
        public float2 CalcSize(Style style)
        {
            if (!hasLabel)
            {
                throw new InvalidOperationException("Label not set");
            }

            if (!style.HasStyle)
            {
                throw new ArgumentException("Style not set");
            }
            
            return style.CalcSize(content);
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