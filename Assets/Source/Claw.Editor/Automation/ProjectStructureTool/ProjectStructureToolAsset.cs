using Beast.Claw.Editor.IO;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.ProjectStructureTool
{
    internal class ProjectStructureToolAsset : ToolAsset
    {
        [SerializeField] private Config _config;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void ScanAndLog()
        {
            UnityConsole console = new(true);
            FileWriter writer = new();
            writer.Write(_config, console);
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        private void ScanAndExport()
        {
            string exportPath = EditorPath.Combine(EditorPath.ProjectRoot, _config.ExportPath.path);
            using FileConsole console = new(exportPath, true);
            FileWriter writer = new();
            writer.Write(_config, console);
            
            UnityEngine.Debug.Log("Done.");
        }
    }
}