using System.Collections.Generic;
using System.IO;
using System.Linq;
using Beast.Claw.Editor.IO;
using Beast.Claw.Text;

namespace Beast.Claw.Editor.Automation.ProjectStructureTool
{
    internal class FileScanner
    {
        private readonly List<RegexPattern> _blacklistFilters; 
        
        private List<EntityInfo> _outEntities;

        public FileScanner()
        {
            _blacklistFilters = new List<RegexPattern>();
        }

        public List<EntityInfo> Scan(Config config)
        {
            _outEntities = new List<EntityInfo>();

            _blacklistFilters.Clear();
            _blacklistFilters.AddRange(config.Blacklist.Where(f => f.enabled).Select(f => f.filter));

            string scanRoot = EditorPath.Combine(EditorPath.ProjectRoot, config.ScanPath.path);
            ScanRecursive(scanRoot, 0);
            
            return _outEntities;
        }

        private void ScanRecursive(string path, int deep)
        {
            if (_blacklistFilters.Any(filter => filter.IsMatch(path)))
            {
                return;
            }
            
            _outEntities.Add(new EntityInfo
            {
                name = Path.GetFileName(path),
                deep = deep,
                sizeBytes = 0
            });

            string[] directories = Directory.GetDirectories(path);
            foreach (string directory in directories)
            {
                ScanRecursive(directory, deep + 1);
            }

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                if (_blacklistFilters.Any(filter => filter.IsMatch(file)))
                {
                    continue;
                }

                FileInfo fileInfo = new(file);
                
                _outEntities.Add(new EntityInfo
                {
                    name = Path.GetFileName(file),
                    deep = deep + 1,
                    sizeBytes = fileInfo.Length
                });
            }
        }
    }
}