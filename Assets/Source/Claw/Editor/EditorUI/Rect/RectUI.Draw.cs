using Beast.Claw.Attributes;
using Beast.Claw.Attributes.Annotation;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class RectUI
    {
        public void Label(Rect rect, [LabelMustBeSet] Label label, Style style)
        {
            label.ValidateHasLabelThrowException();

            if (style.hasStyle)
            {
                GUI.Label(rect, label.content, style.style);
            }
            else
            {
                GUI.Label(rect, label.content);
            }
        }

        public void Texture(Rect rect, Texture texture)
        {
            EditorGUI.DrawPreviewTexture(rect, texture);
        }

        public void Fill(Rect rect, Color color)
        {
            EditorGUI.DrawRect(rect, color);
        }
        
        public bool Button(Rect rect, [LabelCanBeEmpty] Label label, Style style)
        {
            if (label.hasLabel)
            {
                if (style.hasStyle)
                {
                    return GUI.Button(rect, label.content, style.style);
                }
                else
                {
                    return GUI.Button(rect, label.content);
                }
            }
            else
            {
                if (style.hasStyle)
                {
                    return GUI.Button(rect, string.Empty, style.style);
                }
                else
                {
                    return GUI.Button(rect, string.Empty);
                }
            }
        }
    }
}