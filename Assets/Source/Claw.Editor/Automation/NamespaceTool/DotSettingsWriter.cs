using System.Collections.Generic;
using System.IO;

namespace Beast.Claw.Editor.Automation.NamespaceTool
{
    internal class DotSettingsWriter
    {
        public void Write(string toFile, IEnumerable<string> foldersToIgnore)
        {
            using StreamWriter writer = new(toFile);
            
            writer.Write("<wpf:ResourceDictionary xml:space=\"preserve\" ");
            writer.Write("xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" ");
            writer.Write("xmlns:s=\"clr-namespace:System;assembly=mscorlib\" ");
            writer.Write("xmlns:ss=\"urn:shemas-jetbrains-com:settings-storage-xaml\" ");
            writer.Write("xmlns:wpf=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">");
            writer.WriteLine();

            foreach (string path in foldersToIgnore)
            {
                string pathEncoded = path.ToLower().Replace("/", "_005C");

                writer.Write("\t<s:Boolean x:Key=\"/Default/CodeInspection/NamespaceProvider/NamespaceFoldersToSkip/=");
                writer.Write(pathEncoded);
                writer.Write("/@EntryIndexedValue\">True</s:Boolean>");
                writer.WriteLine();
            }
        }
    }
}