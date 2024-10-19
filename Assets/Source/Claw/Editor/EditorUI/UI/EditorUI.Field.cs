using Beast.Claw.Attributes;
using Beast.Claw.Attributes.Annotation;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace Beast.Claw.Editor
{
    partial struct EditorUI
    {
        [NoCustomStyle]
        public Color ColorField([CanBeEmpty] SmartLabel label, Color value)
        {
            ColorField(label, ref value);

            return value;
        }
        
        [NoCustomStyle]
        public void ColorField([CanBeEmpty] SmartLabel label, ref Color value)
        {
            if (label.hasLabel)
            {
                value = EditorGUILayout.ColorField(label.content, value, layoutOptions);
            }
            else
            {
                value = EditorGUILayout.ColorField(value, layoutOptions);
            }
        }

        [CustomStyleSupported]
        public float FloatField([CanBeEmpty] SmartLabel label, float value)
        {
            FloatField(label, ref value);
            
            return value;
        }

        [CustomStyleSupported]
        public void FloatField([CanBeEmpty] SmartLabel label, ref float value)
        {
            if (label.hasLabel)
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.FloatField(label.content, value, style, layoutOptions);
                }
                else
                {
                    value = EditorGUILayout.FloatField(label.content, value, layoutOptions);
                }
            }
            else
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.FloatField(value, style, layoutOptions);
                }
                else
                {
                    value = EditorGUILayout.FloatField(value, layoutOptions);
                }
            }
        }

        [NoCustomStyle]
        public float2 Float2FieldTwoLine([MustHaveLabel] SmartLabel label, float2 value)
        {
            Float2FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float2FieldTwoLine([MustHaveLabel] SmartLabel label, ref float2 value)
        {
            label.ValidateHasLabelThrowException();

            value = EditorGUILayout.Vector2Field(label.content, value, layoutOptions);
        }

        [NoCustomStyle]
        public float2 Float2FieldOneLine([CanBeEmpty] SmartLabel label, float2 value)
        {
            Float2FieldOneLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float2FieldOneLine([CanBeEmpty] SmartLabel label, ref float2 value)
        {
            Rect rect = EditorGUILayout.GetControlRect(false);

            if (label.hasLabel)
            {
                rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), label.content);
            }

            const float LABEL_WIDTH = 15;
            float fieldWidth = (rect.width - LABEL_WIDTH * 2) / 2;

            (Rect w0, Rect w1, Rect w2, Rect w3) = RectSlice.Horizontal(
                rect, LABEL_WIDTH, fieldWidth, LABEL_WIDTH, fieldWidth);
            
            int indentLevelBak = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            EditorGUI.BeginDisabledGroup(true);
            GUIContent tempX = EditorGUIUtility.TrTempContent(" x");
            EditorGUI.PrefixLabel(w0, GUIUtility.GetControlID(FocusType.Passive), tempX);
            
            GUIContent tempY = EditorGUIUtility.TrTempContent(" y");
            EditorGUI.PrefixLabel(w2, GUIUtility.GetControlID(FocusType.Passive), tempY);
            EditorGUI.EndDisabledGroup();
            
            value.x = EditorGUI.FloatField(w1, value.x);
            value.y = EditorGUI.FloatField(w3, value.y);

            EditorGUI.indentLevel = indentLevelBak;
        }
        
        [NoCustomStyle]
        public float3 Float3FieldTwoLine([MustHaveLabel] SmartLabel label, float3 value)
        {
            Float3FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float3FieldTwoLine([MustHaveLabel] SmartLabel label, ref float3 value)
        {
            label.ValidateHasLabelThrowException();

            value = EditorGUILayout.Vector3Field(label.content, value, layoutOptions);
        }
        
        [NoCustomStyle]
        public float4 Float4FieldTwoLine([MustHaveLabel] SmartLabel label, float4 value)
        {
            Float4FieldTwoLine(label, ref value);

            return value;
        }

        [NoCustomStyle]
        public void Float4FieldTwoLine([MustHaveLabel] SmartLabel label, ref float4 value)
        {
            label.ValidateHasLabelThrowException();
            
            value = EditorGUILayout.Vector4Field(label.content, value, layoutOptions);
        }

        [CustomStyleSupported]
        public string StringField([CanBeEmpty] SmartLabel label, string value)
        {
            StringField(label, ref value);

            return value;
        }

        [CustomStyleSupported]
        public void StringField([CanBeEmpty] SmartLabel label, ref string value)
        {
            if (label.hasLabel)
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.TextField(label.content, value, style, layoutOptions);
                }
                else
                {
                    value = EditorGUILayout.TextField(label.content, value, layoutOptions);
                }
            }
            else
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.TextField(value, style, layoutOptions);
                }
                else
                {
                    value = EditorGUILayout.TextField(value, layoutOptions);
                }
            }
        }
        
        [NoCustomStyle]
        public TObject AssetField<TObject>([CanBeEmpty] SmartLabel label, TObject obj) where TObject : Object
        {
            AssetField(label, ref obj);

            return obj;
        }
        
        [NoCustomStyle]
        public void AssetField<TObject>([CanBeEmpty] SmartLabel label, ref TObject obj) where TObject : Object
        {
            if (label.hasLabel)
            {
                obj = (TObject)EditorGUILayout.ObjectField(label.content, obj, typeof(TObject), false, layoutOptions);
            }
            else
            {
                obj = (TObject)EditorGUILayout.ObjectField(obj, typeof(TObject), false, layoutOptions);
            }
        }

        [CustomStyleSupported]
        public int IntField([CanBeEmpty] SmartLabel label, int value)
        {
            IntField(label, ref value);
            
            return value;
        }

        [CustomStyleSupported]
        public void IntField([CanBeEmpty] SmartLabel label, ref int value)
        {
            if (label.hasLabel)
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.IntField(label.content, value, style, layoutOptions);
                }
                else
                {
                    value = EditorGUILayout.IntField(label.content, value, layoutOptions);
                }
            }
            else
            {
                if (flow.TryGetCustomStyle(out GUIStyle style))
                {
                    value = EditorGUILayout.IntField(value, style, layoutOptions);
                }
                else
                {
                    value = EditorGUILayout.IntField(value, layoutOptions);
                }
            }
        }

        [NoCustomStyle]
        public int2 Int2FieldTwoLine([MustHaveLabel] SmartLabel label, int2 value)
        {
            Int2FieldTwoLine(label, ref value);

            return value;
        }
        
        [NoCustomStyle]
        public void Int2FieldTwoLine([MustHaveLabel] SmartLabel label, ref int2 value)
        {
            label.ValidateHasLabelThrowException();

            Vector2 vector = new (value.x, value.y);

            vector = EditorGUILayout.Vector2Field(label.content, vector);

            value = new int2((int)vector.x, (int)vector.y);
        }
    }
}