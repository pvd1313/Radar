using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    partial struct EditorUI
    {
        public void SaveAssetDialog(string fileName, Object obj)
        {
            SaveAssetDialog(fileName, (path) => { AssetDatabase.CreateAsset(obj, path); });
        }

        public void SaveAssetDialog(string fileName, Action<string> action)
        {
            fileName.ValidateNotEmptyThrowException(nameof(fileName));
            action.ValidateNotNullThrowException("SaveAssetDialog : action");

            string path = EditorUtility.SaveFilePanel(
                "Save asset",
                Application.dataPath,
                Path.GetFileNameWithoutExtension(fileName),
                Path.GetExtension(fileName).Replace(".", ""));

            action.Invoke(path);
        }

        public void SaveAssetDialogWithDefaultPath(string fileName, string defaultPath, Action<string> action)
        {
            fileName.ValidateNotEmptyThrowException(nameof(fileName));
            defaultPath.ValidateNotEmptyThrowException(nameof(defaultPath));

            string path = EditorUtility.SaveFilePanel(
                "Save asset",
                defaultPath,
                Path.GetFileNameWithoutExtension(fileName),
                Path.GetExtension(fileName).Replace(".", ""));

            action.Invoke(path);
        }
    }
}