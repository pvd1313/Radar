using System;
using System.IO;
using Beast.Claw.Common;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor
{
    partial struct BeastEditorUI
    {
        public void SaveAssetDialog(string fileName, Object obj)
        {
            SaveAssetDialog(fileName, (path) => { AssetDatabase.CreateAsset(obj, path); });
        }

        public void SaveAssetDialog(string fileName, Action<string> action)
        {
            BAssert.ArgumentNotNull(action, "SaveAssetDialog : action");

            string path = EditorUtility.SaveFilePanel(
                "Save asset",
                Application.dataPath,
                Path.GetFileNameWithoutExtension(fileName),
                Path.GetExtension(fileName).Replace(".", ""));

            action.Invoke(path);
        }
    }
}