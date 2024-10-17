using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    partial class EditorWindow
    {
        protected static bool IsNullOrDirty(Object asset)
        {
            return asset == null || EditorUtility.IsDirty(asset);
        }

        protected static void SetDirty(Object asset)
        {
            asset.ValidateNotNullThrowException(nameof(asset));
            
            EditorUtility.SetDirty(asset);
        }

        protected static void SaveIfDirty(Object asset)
        {
            asset.ValidateNotNullThrowException(nameof(asset));
            
            AssetDatabase.SaveAssetIfDirty(asset);
        }
    }
}