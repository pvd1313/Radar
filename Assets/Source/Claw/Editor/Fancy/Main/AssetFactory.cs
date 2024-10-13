using System.IO;
using Beast.Claw.Common;
using Beast.Claw.Editor.Fancy.Data;
using Beast.Claw.Editor.Fancy.Tools;
using Beast.Claw.Editor.Fancy.Views;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Fancy.Main
{
    internal class AssetFactory : EditorWindow
    {
        private readonly TypeScannerWindow _scannerWindow;
        private readonly RepaintEventsView _repaintEvents;
        
        private string _outputPath;

        [MenuItem("Assets/Create/Fancy...", priority = -1)]
        private static void InvokeWindow()
        {
            WindowUtil.OpenToolWindow<AssetFactory>();
        }

        public AssetFactory()
        {
            _scannerWindow = new TypeScannerWindow(new []{typeof(Scriptable),typeof(ScriptableInstaller)});
            _repaintEvents = new RepaintEventsView(this);
        }

        private void OnEnable()
        {
            _outputPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!AssetDatabase.IsValidFolder(_outputPath))
            {
                _outputPath = Path.GetDirectoryName(_outputPath);
            }
        }

        private void OnGUI()
        {
            _repaintEvents.OnGUI();
            
            WindowUtil.InitializeWindowPosition(this);

            if (!_scannerWindow.OnGUI(out FancyTypeInfo selectedType))
            {
                return;
            }

            Object asset = ScriptableObjectFactory.CreateAsset(_outputPath, selectedType.type);
            
            Selection.activeObject = asset;
            
            Close();
        }

        private void OnLostFocus()
        {
            Close();
        }
    }
}
