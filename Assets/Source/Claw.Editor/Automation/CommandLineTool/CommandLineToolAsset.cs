using System.Diagnostics;
using Beast.Claw.Attributes;
using UnityEngine;

namespace Beast.Claw.Editor.Automation.CommandLineTool
{
    public class CommandLineToolAsset : ToolAsset
    {
        [SerializeField] private CommandLineToolConfig _config;

        [Button]
        private void InvokeCommandLines()
        {
            foreach (string command in _config.CommandLines)
            {
                Process.Start("cmd.exe",  $"/c \"{command}\"");
            }
        }
    }
}