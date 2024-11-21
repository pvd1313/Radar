using System;
using System.Collections.Generic;
using System.IO;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Validation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    [StaticUtility]
    public static class BAsset
    {
        public static string AbsoluteToProjectPath(string path)
        {
            path.ValidateNotEmptyOrNullThrowException(nameof(path));
            
            path = "Assets\\" + Path.GetRelativePath(Application.dataPath, path);

            if (path.Contains(@"\..\"))
            {
                string message = $"'{path}' is not a project asset path";
                throw new InvalidOperationException(message);
            }
            
            return path;
        }

        public static Texture2D SaveTexture(Texture2D texture, string path, Action<TextureImporter> importerOverride = null)
        {
            texture.ValidateNotNullThrowException(nameof(texture));
            path.ValidateNotEmptyOrNullThrowException(nameof(path));
            
            File.WriteAllBytes(path, texture.EncodeToPNG());
            
            AssetDatabase.ImportAsset(path);
            
            if (importerOverride != null)
            {
                TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importerOverride.Invoke(importer);
                importer.SaveAndReimport();
            }

            Texture2D resultTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (resultTexture == null)
            {
                throw new Exception($"Failed to load Texture2D at '{path}'");
            }

            return resultTexture;
        }
        
        public static MonoScript TryFindScript(Type type)
        {
            TryFindScript(type, out MonoScript script);

            return script;
        }

        public static bool TryFindScript(Type type, out MonoScript script)
        {
            string[] guids = AssetDatabase.FindAssets("t:MonoScript " + type.Name);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

                if (script != null && script.GetClass() == type)
                {
                    return true;
                }
            }

            script = null;
            return false;
        }

        public static void TryOpenAsset<TObject>(TObject obj) where TObject : Object
        {
            AssetDatabase.OpenAsset(obj);
        }
        
        public static bool IsNullOrDirty(Object asset)
        {
            return asset == null || EditorUtility.IsDirty(asset);
        }

        public static bool IsDirty(Object asset)
        {
            return EditorUtility.IsDirty(asset);
        }

        public static void SetDirty(Object asset)
        {
            asset.ValidateNotNullThrowException(nameof(asset));
            
            EditorUtility.SetDirty(asset);
        }

        public static void SaveIfDirty(Object asset)
        {
            asset.ValidateNotNullThrowException(nameof(asset));
            
            AssetDatabase.SaveAssetIfDirty(asset);
        }

        [PerformanceCritical]
        public static void FindAssets<TAsset>(List<AssetInfo> result) where TAsset : Object
        {
            result.ValidateNotNullThrowException(nameof(result));
            result.Clear();
            
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(TAsset).Name}");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
                
                if (asset != null)
                {
                    result.Add(new AssetInfo
                    {
                        asset = asset,
                        guid = guid,
                        path = path
                    });
                }
            }
        }

        /// <summary>
        /// Uses UnityEngine.Resources.FindObjectsOfTypeAll();
        /// </summary>
        [PerformanceCritical]
        public static void RepaintEditorWindows()
        {
            EditorWindow[] windows = UnityEngine.Resources.FindObjectsOfTypeAll<EditorWindow>();

            foreach (EditorWindow window in windows)
            {
                if (window.hasFocus)
                {
                    window.Repaint();
                }
            }
        }

        public static int DuplicateArrayElement<TAsset>(TAsset asset, string arrayPath, int sourceElementIndex)
            where TAsset : Object
        {
            SerializedObject serializedObject = new (asset);
            
            SerializedProperty array = serializedObject.FindProperty(arrayPath);
            if (array == null)
            {
                throw new Exception($"Failed to find '{arrayPath}' in {typeof(TAsset).FullName}");
            }

            if (!array.isArray)
            {
                throw new Exception($"'{arrayPath}' is not an array ({typeof(TAsset).FullName})");
            }
            
            SerializedProperty source = array.GetArrayElementAtIndex(sourceElementIndex);
            if (source == null)
            {
                string message =
                    $"Failed to get element ({sourceElementIndex}) in array '{arrayPath}' ({typeof(TAsset).FullName})";

                throw new InvalidOperationException(message);
            }

            bool success = source.DuplicateCommand();

            if (!success)
            {
                string message =
                    $"Failed to copy array element ({sourceElementIndex}) in array '{arrayPath}' ({typeof(TAsset).FullName}" 
                    + "SerializedProperty.DuplicateCommand() returned false.";

                throw new Exception(message);
            }

            serializedObject.ApplyModifiedProperties();

            return sourceElementIndex + 1;
        }
    }
}