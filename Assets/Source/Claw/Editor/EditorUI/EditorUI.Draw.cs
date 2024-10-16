using System;
using Beast.Claw.Math;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    partial struct EditorUI
    {
        public bool Foldout(string label, bool isFold)
        {
            return EditorGUILayout.Foldout(isFold, label);
        }

        public void Foldout(string label, ref bool isFold)
        {
            isFold = EditorGUILayout.Foldout(isFold, label);
        }

        public void Space(float pixelSize)
        {
            GUILayout.Space(pixelSize);
        }

        public void WarningBox(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        public void InfoBox(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }
        
        public void Button(string label, Action action)
        {
            action.ValidateNotNullThrowException($"{flow.NicifyLabel(label)} : action");

            if (GUILayout.Button(flow.NicifyLabel(label), layoutOptions))
            {
                action.Invoke();
            }
        }
        
        public void Button(string label, Action<EditorUI> action)
        {
            action.ValidateNotNullThrowException($"{flow.NicifyLabel(label)} : action");

            if (GUILayout.Button(flow.NicifyLabel(label), layoutOptions))
            {
                action.Invoke(this);
            }
        }

        public void ErrorBox(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }

        public T EnumSingleField<T>(string label, T value) where T : Enum
        {
            return (T)EditorGUILayout.EnumPopup(flow.NicifyLabel(label), value, layoutOptions);
        }
        
        public void EnumSingleField<T>(string label, ref T value) where T : Enum
        {
            value = (T)EditorGUILayout.EnumPopup(flow.NicifyLabel(label), value, layoutOptions);
        }

        public T EnumFlagsField<T>(string label, T value) where T : Enum
        {
            return (T)EditorGUILayout.EnumFlagsField(flow.NicifyLabel(label), value, layoutOptions);
        }
        
        public void EnumFlagsField<T>(string label, ref T value) where T : Enum
        {
            value = (T)EditorGUILayout.EnumFlagsField(flow.NicifyLabel(label), value, layoutOptions);
        }

        public void TexturePreview(string label, Texture texture, float height)
        {
            if (!string.IsNullOrEmpty(label))
            {
                EditorGUILayout.LabelField(flow.NicifyLabel(label));
            }

            Rect rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }

        public int IntField(string label, int value)
        {
            return EditorGUILayout.IntField(flow.NicifyLabel(label), value, layoutOptions);
        }
        
        public void IntField(string label, ref int value)
        {
            value = EditorGUILayout.IntField(flow.NicifyLabel(label), value, layoutOptions);
        }

        public void Label(string label)
        {
            EditorGUILayout.LabelField(label);
        }

        public float FloatField(string label, float value)
        {
            return EditorGUILayout.FloatField(flow.NicifyLabel(label), value, layoutOptions);
        }

        public Color ColorField(string label, Color value)
        {
            return EditorGUILayout.ColorField(flow.NicifyLabel(label), value);
        }
        
        public void ColorField(string label, ref Color value)
        {
            value = EditorGUILayout.ColorField(flow.NicifyLabel(label), value);
        }
        
        public void FloatField(string label, ref float value)
        {
            value = EditorGUILayout.FloatField(flow.NicifyLabel(label), value, layoutOptions);
        }

        public float2 Float2Field(string label, float2 value)
        {
            return EditorGUILayout.Vector2Field(label, value);
        }

        public float2 Float2FieldOneLine(string label, float2 value)
        {
            Rect rect = EditorGUILayout.GetControlRect(false);

            GUIContent temp = EditorGUIUtility.TrTempContent(label);
            rect = EditorGUI.PrefixLabel(rect, GUIUtility.GetControlID(FocusType.Passive), temp);

            const float labelWidth = 15;
            float fieldWidth = (rect.width - labelWidth * 2) / 2;

            (Rect w0, Rect w1, Rect w2, Rect w3) = RectSlicer.Horizontal(
                rect, labelWidth, fieldWidth, labelWidth, fieldWidth);
            
            int indentLevelBak = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            EditorGUI.BeginDisabledGroup(true);
            temp = EditorGUIUtility.TrTempContent(" x");
            EditorGUI.PrefixLabel(w0, GUIUtility.GetControlID(FocusType.Passive), temp);
            
            temp = EditorGUIUtility.TrTempContent(" y");
            EditorGUI.PrefixLabel(w2, GUIUtility.GetControlID(FocusType.Passive), temp);
            EditorGUI.EndDisabledGroup();
            
            float x = EditorGUI.FloatField(w1, value.x);
            float y = EditorGUI.FloatField(w3, value.y);
            

            EditorGUI.indentLevel = indentLevelBak;

            return new float2(x, y);
        }

        public bool CheckBoxRight(string label, bool value)
        {
            return EditorGUILayout.Toggle(flow.NicifyLabel(label), value, layoutOptions);
        }
        
        public void CheckBoxRight(string label, ref bool value)
        {
            value = EditorGUILayout.Toggle(flow.NicifyLabel(label), value, layoutOptions);
        }
        
        public bool CheckBoxLeft(string label, bool value)
        {
            return EditorGUILayout.Toggle(flow.NicifyLabel(label), value, layoutOptions);
        }
        
        public void CheckBoxLeft(string label, ref bool value)
        {
            value = EditorGUILayout.ToggleLeft(flow.NicifyLabel(label), value, layoutOptions);
        }

        public void FlexibleSpace()
        {
            GUILayout.FlexibleSpace();
        }
        
        public float3 Float3Field(string label, float3 value)
        {
            return EditorGUILayout.Vector3Field(flow.NicifyLabel(label), value, layoutOptions);
        }
        
        public float4 Float4Field(string label, float4 value)
        {
            return EditorGUILayout.Vector4Field(flow.NicifyLabel(label), value, layoutOptions);
        }

        public string StringField(string label, string value)
        {
            return EditorGUILayout.TextField(label, value, layoutOptions);
        }
        
        public void StringField(string label, ref string value)
        {
            value = EditorGUILayout.TextField(label, value, layoutOptions);
        }

        public T AssetField<T>(string label, T obj) where T : Object
        {
            return (T)EditorGUILayout.ObjectField(flow.NicifyLabel(label), obj, typeof(T), false, layoutOptions);
        }
        
        public void AssetField<T>(string label, ref T obj) where T : Object
        {
            obj = (T)EditorGUILayout.ObjectField(flow.NicifyLabel(label), obj, typeof(T), false, layoutOptions);
        }

        public int2 Int2Field(string label, int2 value)
        {
            Vector2 vector = new(value.x, value.y);

            vector = EditorGUILayout.Vector2Field(flow.NicifyLabel(label), vector, layoutOptions);

            return new int2(BMath.FloorToInt(vector.x), BMath.FloorToInt(vector.y));
        }
        
        public void Int2Field(string label, ref int2 value)
        {
            Vector2 vector = new(value.x, value.y);

            vector = EditorGUILayout.Vector2Field(flow.NicifyLabel(label), vector, layoutOptions);

            value = new int2(BMath.FloorToInt(vector.x), BMath.FloorToInt(vector.y));
        }
    }
}