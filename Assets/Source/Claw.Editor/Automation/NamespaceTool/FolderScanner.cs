using System.Collections.Generic;
using System.IO;
using Beast.Claw.Editor.IO;

namespace Beast.Claw.Editor.Automation.NamespaceTool
{
    internal class FolderScanner
    {
        private HashSet<FolderInfo> _outFolders;
        private IReadOnlyList<FilterInfo> _filters;
        private int _rootLength;
        
        public HashSet<FolderInfo> Scan(Config config)
        {
            _outFolders = new HashSet<FolderInfo>();
            _filters = config.FolderFilters;
            _rootLength = EditorPath.ProjectRoot.Length + 1;
            
            string path = EditorPath.Combine(EditorPath.ProjectRoot, config.ScanPath.path);
            ScanRecursive(path);

            return _outFolders;
        }

        private void ScanRecursive(string path)
        {
            for (int i = 0; i < _filters.Count; i++)
            {
                FilterInfo filter = _filters[i];
                if (!filter.enabled)
                {
                    continue;
                }

                if (!filter.filter.IsMatch(path))
                {
                    continue;
                }

                _outFolders.Add(new FolderInfo
                {
                    name = path.Substring(_rootLength),
                    regexId = i
                });
                
                break;
            }
            
            string[] subDirectories = Directory.GetDirectories(path);
            foreach (string directory in subDirectories)
            {
                ScanRecursive(path + "/" + Path.GetFileName(directory));
            }
        }
    }
}