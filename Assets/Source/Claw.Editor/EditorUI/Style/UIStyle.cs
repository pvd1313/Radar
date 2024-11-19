using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public static class UIStyle
    {
        public const string CHAR_ARROW_UP = "↑";
        public const string CHAR_ARROW_DOWN = "↓";
        public const string CHAR_PLUS = "+";
        public const string CHAR_CROSS = "x";
        public const string CHAR_A = "A";
        
        public const int SIZE_ONE_CHAR = 20;
        public const int SIZE_TWO_CHAR = 25;
        public const int SIZE_MINOR_SPACE = 8;
        public const int SIZE_ASSET_FIELD = 140;
        
        public const string ICON_CSHARP_SCRIPT = "d_cs Script Icon";
        public const string ICON_CONSOLE_INFO = "console.infoicon";
        
        public static Color EditorSkinBackgroundColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return new Color(0.2196079f, 0.2196079f, 0.2196079f);
                }

                return new Color(0.7843138f, 0.7843138f, 0.7843138f);
            }
        }
    }
}