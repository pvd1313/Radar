using System;
using System.IO;
using Beast.Claw.Common;
using Beast.Claw.Mathematics;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Playables;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    public abstract partial class BeastEditorWindow
    {
        protected static class UI
        {
            public static T AssetField<T>(string label, T obj) where T : Object
            {
                return (T)EditorGUILayout.ObjectField(NicifyLabel(label), obj, typeof(T), false);
            }

            public static int2 Int2Field(string label, int2 value)
            {
                Vector2 vector = new (value.x, value.y);

                vector = EditorGUILayout.Vector2Field(NicifyLabel(label), vector);

                return new int2(BMath.FloorToInt(vector.x), BMath.FloorToInt(vector.y));
            }

            public static void TexturePreview(string label, Texture texture, float height)
            {
                if (!string.IsNullOrEmpty(label))
                {
                    EditorGUILayout.LabelField(NicifyLabel(label));
                }
                
                Rect rect = EditorGUILayout.GetControlRect(false, height);
                EditorGUI.DrawPreviewTexture(rect, texture, null, ScaleMode.ScaleToFit);
            }

            public static int IntField(string label, int value)
            {
                return EditorGUILayout.IntField(NicifyLabel(label), value);
            }
            
            public static void SaveAssetDialog(string fileName, Object obj)
            {
                SaveAssetDialog(fileName, (path) =>
                {
                    AssetDatabase.CreateAsset(obj, path);
                });
            }

            public static void SaveAssetDialog(string fileName, Action<string> action)
            {
                string path = EditorUtility.SaveFilePanel(
                    "Save asset",
                    Application.dataPath,
                    Path.GetFileNameWithoutExtension(fileName),
                    Path.GetExtension(fileName).Replace(".", ""));

                action.Invoke(path);
            }

            public static T EnumSingleField<T>(string label, T value) where T : Enum
            {
                return (T) EditorGUILayout.EnumPopup(NicifyLabel(label), value);
            }

            public static T EnumFlagsField<T>(string label, T value) where T : Enum
            {
                return (T) EditorGUILayout.EnumFlagsField(NicifyLabel(label), value);
            }

            public static void Button(string label, Action action)
            {
                BeastAssert.NotNull(action, $"{NicifyLabel(label)} : action");
                
                if (GUILayout.Button(NicifyLabel(label)))
                {
                    action.Invoke();
                }
            }

            public static void ErrorBox(string message)
            {
                EditorGUILayout.HelpBox(message, MessageType.Error);
            }

            private static string NicifyLabel(string name)
            {
                return ObjectNames.NicifyVariableName(name);
            }
        }
    }
}