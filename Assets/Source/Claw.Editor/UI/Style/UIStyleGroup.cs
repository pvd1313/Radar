using System;
using Beast.Claw.Debug;
using Beast.Claw.Editor.UI.UI;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public struct UIStyleGroup
    {
        public Style Label { get; private set; }
        public Style Button { get; private set; }
        public Style TextField { get; private set; }
        public Style NumberField { get; private set; }
        public Style SelectionBox { get; private set; }
        
        public float MinorSpacing { get; private set; }

        public static UIStyleGroup Default()
        {
            try
            {
                return new UIStyleGroup
                {
                    Label = EditorStyles.label,
                    TextField = EditorStyles.textField,
                    NumberField = EditorStyles.numberField,
                    SelectionBox = EditorStyles.selectionRect,
                    Button = GUI.skin.button,
                    MinorSpacing = 5
                };
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }

            return default;
        }
    }
}