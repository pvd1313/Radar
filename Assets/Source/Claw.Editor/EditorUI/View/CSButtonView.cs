using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public class CSButtonView
    {
        public GUILayoutOption ButtonHeight => _buttonHeight;
        
        private readonly IFlow _flow;
        
        private GUILayoutOption _buttonWidth;
        private GUILayoutOption _buttonHeight;
        private GUIContent _csIcon;
        private GUIContent _infoIcon;
        private GUIStyle _buttonStyle;
        
        public CSButtonView(IFlow flow)
        {
            _flow = flow;
            _buttonWidth = GUILayout.Width(UIStyle.SIZE_ONE_CHAR);
            _buttonHeight = GUILayout.Height(UIStyle.SIZE_ONE_CHAR);
            
            _csIcon = EditorGUIUtility.IconContent(UIStyle.ICON_CSHARP_SCRIPT);
            _infoIcon = EditorGUIUtility.IconContent(UIStyle.ICON_CONSOLE_INFO);

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(0, 0, 0, 0),
                imagePosition = ImagePosition.ImageOnly
            };
        }

        public void DrawAsModeSwitch(ref bool csModeEnabled)
        {
            bool modeEnabled = csModeEnabled;
            
            _flow.Begin()
                .Option(_buttonWidth, _buttonHeight)
                .Style(_buttonStyle)
                .Draw(ui =>
            {
                ui.Button(modeEnabled ? _csIcon : _infoIcon, () =>
                {
                    modeEnabled = !modeEnabled;
                });
            });

            csModeEnabled = modeEnabled;
        }

        public void DrawAsScriptLink(MonoScript script, bool hasScript)
        {
            _flow.Begin()
                .Enabled(hasScript)
                .Option(_buttonWidth, _buttonHeight)
                .Style(_buttonStyle)
                .Draw(ui =>
                {
                    ui.Button(_csIcon, () =>
                    {
                        BAsset.TryOpenAsset(script);
                    });
                });
        }
    }
}