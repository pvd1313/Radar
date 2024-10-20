using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    readonly partial struct EditorUI
    {
        public void SaveFileDialog(string fileName, Action<string> action)
        {
            fileName.ValidateNotEmptyOrNullThrowException(nameof(fileName));
            action.ValidateNotNullThrowException("SaveAssetDialog : action");

            string path = EditorUtility.SaveFilePanel(
                "Save asset",
                Application.dataPath,
                Path.GetFileNameWithoutExtension(fileName),
                Path.GetExtension(fileName).Replace(".", ""));

            action.Invoke(path);
        }
        
        public string SaveFileDialog(string fileName, string defaultPath)
        {
            fileName.ValidateNotEmptyOrNullThrowException(nameof(fileName));
            defaultPath.ValidateNotEmptyOrNullThrowException(nameof(defaultPath));

            return EditorUtility.SaveFilePanel(
                "Save asset",
                defaultPath,
                Path.GetFileNameWithoutExtension(fileName),
                Path.GetExtension(fileName).Replace(".", ""));
        }
    }
}