using System;
using System.IO;
using System.Text.RegularExpressions;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Editor.IO
{
    [Serializable]
    public struct EditorFolderPath : IValidatable
    {
        private const string BLACKLIST_REGEX = @"(\\)|(\/$)|(^\/)|(\/\/)";
        
        [SerializeField] public string path;
        
        public string Validate()
        {
            if (path is null)
            {
                return $"'{nameof(path)}' is null";
            }

            Match match = Regex.Match(path, BLACKLIST_REGEX);
            if (match.Success)
            {
                return $"'{nameof(path)}' contains|starts|ends with '{match.Value}'";
            }

            string folder = EditorPath.Combine(EditorPath.ProjectRoot, path);
            if (!Directory.Exists(folder))
            {
                return $"Folder '{folder}' does not exist";
            }

            return string.Empty;
        }
    }
}