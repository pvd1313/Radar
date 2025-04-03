using System;
using System.IO;
using System.Text.RegularExpressions;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Editor.IO
{
    [Serializable]
    public struct EditorFilePath : IValidatable
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

            string file = EditorPath.Combine(EditorPath.ProjectRoot, path);
            if (!File.Exists(file))
            {
                return $"File '{file}' does not exist";
            }
            
            return null;
        }
    }
}