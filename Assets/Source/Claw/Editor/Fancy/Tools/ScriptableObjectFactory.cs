using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Fancy.Tools
{
    internal static class ScriptableObjectFactory
    {
        private const int MaxAssetNameGenerationIterations = 1024;
        
        public static UnityEngine.Object CreateAsset(string directory, Type type)
        {
            string path = Path.Combine(directory, type.Name + ".asset");

            if (CheckAssetExists(path))
            {
                path = Path.Combine(directory, type.Name + " - new.asset");
            }

            for (int i = 0; CheckAssetExists(path); i++)
            {
                if (i >= MaxAssetNameGenerationIterations)
                {
                    Debug.LogError($"Failed to create asset '{type}' at '{directory}'. All names are occupied");
                    return null;
                }
                
                path = Path.Combine(directory, type.Name + " - new (" + i + ").asset");
            }

            try
            {
                ScriptableObject asset = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(asset, path);
                return asset;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return null;
        }
        
        private static bool CheckAssetExists(string path)
        {
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path));
        }
    }
}