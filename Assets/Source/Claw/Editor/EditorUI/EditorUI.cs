using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial struct EditorUI
    {
        public const string CharUpArrow = "↑";
        public const string CharDownArrow = "↓";
        public const string CharPlus = "+";
        public const string CharCross = "x";
        public const string CharA = "A"; 
        public const int OneCharButtonSize = 20;
        public const int TwoCharButtonSize = 25;
        public const int MinorSpace = 8;

        public EditorFlow flow;
        public GUILayoutOption[] layoutOptions;
    }
}