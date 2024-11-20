using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Beast.Claw.Editor.GoogleSheet
{
    public class ImporterConfig : ScriptableObject
    {
        public string fileId;
        public string outputPath;
        public List<string> sheets;

        #if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
        #endif
        private void Import()
        {
            string path = Path.Combine(Application.dataPath, outputPath);

            GoogleSheetImporter.AsyncImportRecursive(this, path, 0, 0);
        }
    }
}