using System;
using Beast.Claw.Attributes;
using Beast.Claw.Attributes.Annotation;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial struct EditorUI
    {
        [CustomStyleSupported]
        public void Foldout([MustHaveLabel] SmartLabel label, ref bool isFold)
        {
            label.ValidateHasLabelThrowException();
            
            if (flow.TryGetCustomStyle(out GUIStyle style))
            {
                isFold = EditorGUILayout.Foldout(isFold, label.content, style);
            }
            
            isFold = EditorGUILayout.Foldout(isFold, label.content);
        }

        [NoCustomStyle]
        public void Space(float pixelSize)
        {
            GUILayout.Space(pixelSize);
        }
        
        [NoCustomStyle]
        public void InfoBox(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }

        [NoCustomStyle]
        public void WarningBox(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }
        
        [NoCustomStyle]
        public void ErrorBox(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }
        
        [CustomStyleSupported]
        public void Button([MustHaveLabel] SmartLabel label, Action<EditorUI> action)
        {
            label.ValidateHasLabelThrowException();
            action.ValidateNotNullThrowException("label : action");
            
            if (flow.TryGetCustomStyle(out GUIStyle style))
            {
                if (GUILayout.Button(label.content, style, layoutOptions))
                {
                    action.Invoke(this);
                }
            }
            else
            {
                if (GUILayout.Button(label.content, layoutOptions))
                {
                    action.Invoke(this);
                }
            }
        }
        
        [CustomStyleSupported]
        public void Button([MustHaveLabel] SmartLabel label, Action action)
        {
            label.ValidateHasLabelThrowException();
            action.ValidateNotNullThrowException("label : action");

            if (flow.TryGetCustomStyle(out GUIStyle style))
            {
                if (GUILayout.Button(label.content, style, layoutOptions))
                {
                    action.Invoke();
                }
                
                return;
            }
            else
            {
                if (GUILayout.Button(label.content, layoutOptions))
                {
                    action.Invoke();
                }
            }
        }
        
        [CustomStyleSupported]
        public void EnumSingleField<T>([CanBeEmpty] SmartLabel label, ref T value) where T : Enum
        {
            if (label.hasLabel)
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = (T)EditorGUILayout.EnumPopup(label.content, value, style, layoutOptions);
                }
                else
                {
                    value = (T)EditorGUILayout.EnumPopup(label.content, value, layoutOptions);
                }
            }
            else
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = (T)EditorGUILayout.EnumPopup(value, style, layoutOptions);
                }
                else
                {
                    value = (T)EditorGUILayout.EnumPopup(value, layoutOptions);
                }
            }
        }
        
        [CustomStyleSupported]
        public void EnumFlagsField<T>([CanBeEmpty] SmartLabel label, ref T value) where T : Enum
        {
            if (label.hasLabel)
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = (T)EditorGUILayout.EnumFlagsField(label.content, value, style, layoutOptions);
                }
                else
                {
                    value = (T)EditorGUILayout.EnumFlagsField(label.content, value, layoutOptions);
                }
            }
            else
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = (T)EditorGUILayout.EnumFlagsField(value, style, layoutOptions);
                }
                else
                {
                    value = (T)EditorGUILayout.EnumFlagsField(value, layoutOptions);
                }
            }
        }

        [NoCustomStyle]
        public void TexturePreview([NotNull] Texture texture, float height)
        {
            texture.ValidateNotNullThrowException(nameof(texture));
            
            Rect rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }

        [CustomStyleSupported]
        public void PassiveLabel([MustHaveLabel] SmartLabel label)
        {
            label.ValidateHasLabelThrowException();

            if (flow.TryGetCustomStyle(out GUIStyle style))
            {
                EditorGUILayout.LabelField(label.content, style, layoutOptions);
            }
            else
            {
                EditorGUILayout.LabelField(label.content);
            }
        }
        
        [CustomStyleSupported]
        public void CheckBoxRight([CanBeEmpty] SmartLabel label, ref bool value)
        {
            if (label.hasLabel)
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.Toggle(label.content, value, style, layoutOptions);
                }
                else
                {
                    value = EditorGUILayout.Toggle(label.content, value, layoutOptions);
                }
            }
            else
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.Toggle(value, style, layoutOptions);
                }
                else
                {
                    value = EditorGUILayout.Toggle(value, layoutOptions);
                }
            }
            
        }

        [NoCustomStyle]
        public void CheckBoxLeft([MustHaveLabel] SmartLabel label, ref bool value)
        {
            label.ValidateHasLabelThrowException();

            if (flow.TryGetCustomStyle(out GUIStyle style))
            {
                value = EditorGUILayout.ToggleLeft(label.content, value, style, layoutOptions);
            }
            else
            {
                value = EditorGUILayout.ToggleLeft(label.content, value, layoutOptions);
            }
        }

        public void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }
    }
}