using System.Text.RegularExpressions;
using Beast.Claw.Editor.Fancy.Data;
using Beast.Claw.Editor.Fancy.Tools;
using Beast.Claw.Editor.Fancy.Views;
using UnityEditor;

namespace Beast.Claw.Editor.Fancy.Main
{
    internal class WindowFactory : EditorWindow
    {
        private const string NameRegex = "(Window$)|([A-Z][a-z0-9]+)";

        private readonly TypeScannerWindow _scannerWindow;
        private readonly RepaintEventsView _repaintEvents;
        
        [MenuItem("Window/Fancy...", priority = int.MinValue)]
        private static void OpenWindow()
        {
            WindowUtil.OpenToolWindow<WindowFactory>();
        }

        public WindowFactory()
        {
            _scannerWindow = new TypeScannerWindow(typeof(EditorWindow));
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

            string windowTitle = Regex.Replace(selectedType.nameShort, NameRegex, "$2 ").Trim();
            
            GetWindow(selectedType.type, false, windowTitle);
            
            Close();
        }

        private void OnLostFocus()
        {
            Close();
        }
    }
}