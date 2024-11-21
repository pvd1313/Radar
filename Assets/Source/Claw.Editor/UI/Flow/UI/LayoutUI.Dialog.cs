using System;
using System.IO;
using Beast.Claw.Text;
using Beast.Claw.Validation;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.UI.UI
{
    readonly partial struct FlowUI
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
            
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            action.Invoke(path);
        }
        
        public void SaveFileDialogWithMemory(string fileName, ref string lastPath, Action<string> action)
        {
            fileName.ValidateNotEmptyOrNullThrowException(nameof(fileName));
            action.ValidateNotNullThrowException("SaveAssetDialog : action");

            string path = EditorUtility.SaveFilePanel(
                "Save asset",
                !lastPath.IsNullOrEmpty() ? lastPath : Application.dataPath,
                Path.GetFileNameWithoutExtension(fileName),
                Path.GetExtension(fileName).Replace(".", ""));

            if (path.IsNullOrEmpty())
            {
                return;
            }

            lastPath = path;

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