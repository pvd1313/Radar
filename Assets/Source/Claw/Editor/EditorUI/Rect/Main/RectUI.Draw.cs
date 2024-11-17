using System;
using Beast.Claw.Common;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class RectUI
    {
        public UIStyleGroup Style => _style;
        
        public IDisposable RemoveIndent()
        {
            return _indentPool.Next().RemoveIndent();
        }

        public IDisposable RemoveInteraction()
        {
            return _indentPool.Next().RemoveInteraction();
        }
        
        public void Label(Rect screen, Label label, Style style)
        {
            label.ValidateHasLabelThrowException();
            style.ValidateHasStyleThrowException();
            
            GUI.Label(screen, label.content, style);
        }

        public Rect PrefixLabel(Rect screen, Label label, Style style)
        {
            label.ValidateHasLabelThrowException();
            style.ValidateHasStyleThrowException();
            
            return EditorGUI.PrefixLabel(screen, label.content, style);
        }

        public float FloatField(Rect screen, float value, Style style)
        {
            style.ValidateHasStyleThrowException();

            return EditorGUI.FloatField(screen, value, style);
        }

        public void Texture(Rect rect, Texture texture)
        {
            texture.ValidateNotNullThrowException(nameof(texture));
            
            EditorGUI.DrawPreviewTexture(rect, texture);
        }

        public void Fill(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
        }
        
        public bool Button(Rect rect, Label label, Style style)
        {
            return GUI.Button(rect, label.content, style);
        }
    }
}