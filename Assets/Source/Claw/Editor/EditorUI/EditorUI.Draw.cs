using System;
using Beast.Claw.Common;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    partial struct EditorUI
    {
        public void Button(string label, Action action)
        {
            BAssert.ArgumentNotNull(action, $"{NicifyLabel(label)} : action");

            if (GUILayout.Button(NicifyLabel(label)))
            {
                action.Invoke();
            }
        }

        public void ErrorBox(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }

        public T EnumSingleField<T>(string label, T value) where T : Enum
        {
            return (T)EditorGUILayout.EnumPopup(NicifyLabel(label), value);
        }

        public T EnumFlagsField<T>(string label, T value) where T : Enum
        {
            return (T)EditorGUILayout.EnumFlagsField(NicifyLabel(label), value);
        }

        public void TexturePreview(string label, Texture texture, float height)
        {
            if (!string.IsNullOrEmpty(label))
            {
                EditorGUILayout.LabelField(NicifyLabel(label));
            }

            Rect rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
        }

        public int IntField(string label, int value)
        {
            return EditorGUILayout.IntField(NicifyLabel(label), value);
        }

        public float FloatField(string label, float value)
        {
            return EditorGUILayout.FloatField(NicifyLabel(label), value);
        }

        public float2 Float2Field(string label, float2 value)
        {
            return EditorGUILayout.Vector2Field(NicifyLabel(label), value);
        }
        
        public float3 Float3Field(string label, float3 value)
        {
            return EditorGUILayout.Vector3Field(NicifyLabel(label), value);
        }
        
        public float4 Float4Field(string label, float4 value)
        {
            return EditorGUILayout.Vector4Field(NicifyLabel(label), value);
        }

        public string StringField(string label, string value)
        {
            return EditorGUILayout.TextField(label, value);
        }

        public T AssetField<T>(string label, T obj) where T : Object
        {
            return (T)EditorGUILayout.ObjectField(NicifyLabel(label), obj, typeof(T), false);
        }

        public int2 Int2Field(string label, int2 value)
        {
            Vector2 vector = new(value.x, value.y);

            vector = EditorGUILayout.Vector2Field(NicifyLabel(label), vector);

            return new int2(BMath.FloorToInt(vector.x), BMath.FloorToInt(vector.y));
        }
    }
}