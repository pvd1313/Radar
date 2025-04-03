#if ODIN_INSPECTOR

using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.IO
{
    public class EditorFolderPathDrawer : OdinValueDrawer<EditorFolderPath>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            bool hasLabel = label != null;
            Rect rect = EditorGUILayout.GetControlRect(false, 20);
            EditorFolderPath value = ValueEntry.SmartValue;
            string valueName = hasLabel ? label.text : ObjectNames.NicifyVariableName(nameof(value.path));
            value.path = EditorGUI.TextField(rect, valueName, value.path);
            
            ValueEntry.SmartValue = value;
        }
    }
}

#endif