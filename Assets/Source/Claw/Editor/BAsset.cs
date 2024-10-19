using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
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
    }
}