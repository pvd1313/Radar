using System.Text.RegularExpressions;
using UnityEditor;

namespace Beast.Claw.Editor.Fancy
{
    internal class WindowFactory : EditorWindow
    {
        private const string NAME_REGEX = "(Window$)|([A-Z][a-z0-9]+)";

        private readonly TypeScannerWindow _scannerWindow;
        private readonly RepaintEventsView _repaintEvents;
        
        [MenuItem("Window/Fancy...", priority = int.MinValue)]
        private static void OpenWindow()
        {
            WindowUtil.OpenToolWindow<WindowFactory>();
        }

        public WindowFactory()
        {
            _scannerWindow = new TypeScannerWindow(new[] { typeof(EditorWindow) });
            _repaintEvents = new RepaintEventsView(this);
        }

        private void OnGUI()
        {
            _repaintEvents.OnGUI();
            
            WindowUtil.InitializeWindowPosition(this);

            if (!_scannerWindow.OnGUI(out FancyTypeInfo selectedType))
            {
                return;
            }

            string windowTitle = Regex.Replace(selectedType.nameShort, NAME_REGEX, "$2 ").Trim();
            
            GetWindow(selectedType.type, false, windowTitle);
            
            Close();
        }

        private void OnLostFocus()
        {
            Close();
        }
    }
}