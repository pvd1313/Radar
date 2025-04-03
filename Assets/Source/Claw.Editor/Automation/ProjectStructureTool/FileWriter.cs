using System;
using System.Collections.Generic;
using System.Linq;

namespace Beast.Claw.Editor.Automation.ProjectStructureTool
{
    internal class FileWriter
    {
        private string[] _pads;
        
        public void Write(Config config, IConsole console)
        {
            FileScanner scanner = new();
            List<EntityInfo> entities = scanner.Scan(config);
            int deepMax = entities.Max(f => f.deep);
            
            _pads = new string[deepMax + 1];
            for (int i = 0; i < _pads.Length; i++)
            {
                _pads[i] = string.Join(string.Empty, Enumerable.Repeat("|   ", i));
            }
            
            foreach (EntityInfo entity in entities)
            {
                string text = FormatEntity(entity);
                console.WriteLine(text);
            }

            int fileCount = entities.Count(e => e.sizeBytes > 0);
            int directoryCount = entities.Count(e => e.sizeBytes == 0);
            long filesSizeTotal = entities.Sum(e => e.sizeBytes);
            
            console.WriteLine(string.Empty);
            console.WriteLine("Summary:");
            console.WriteLine($"\tTime: {DateTime.UtcNow:dd MMMM yyyy HH:mm:ss}");
            console.WriteLine($"\tDirectory count: {directoryCount}");
            console.WriteLine($"\tFile count: {fileCount}");
            console.WriteLine($"\tFiles size total: {FormatBytes(filesSizeTotal)}");
        }

        private string FormatEntity(EntityInfo entity)
        {
            if (entity.sizeBytes == 0)
            {
                return $"{_pads[entity.deep]}{entity.name}";
            }
            
            return $"{_pads[entity.deep]}{entity.name} ({FormatBytes(entity.sizeBytes)})";
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1000)
            {
                return $"{bytes}b";
            }
            
            float size = bytes / 1024f;
            return $"{size:N1}kb";
        }
    }
}