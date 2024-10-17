using System.Diagnostics.CodeAnalysis;
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

        protected static void SetDirty([NotNull] Object asset)
        {
            EditorUtility.SetDirty(asset);
        }

        protected static void SaveIfDirty([NotNull] Object asset)
        {
            AssetDatabase.SaveAssetIfDirty(asset);
        }
    }
}