using System;
using Beast.Claw.Debug;
using UnityEditor;

namespace Beast.Claw.Editor
{
    public struct UIStyleGroup
    {
        public Style Label { get; private set; }
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
                    MinorSpacing = 5
                };
            }
            catch (Exception e)
            {
                BDebug.StaticLogException(e);
            }

            return default;
        }
    }
}