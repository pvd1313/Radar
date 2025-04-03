using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Compilation;

namespace Beast.Claw.Editor.Automation.NamespaceTool
{
    internal class AssemblyUtil
    {
        public List<AssemblyInfo> Scan(Config config)
        {
            List<Assembly> assemblies = new();
            assemblies.AddRange(CompilationPipeline.GetAssemblies(AssembliesType.Player));
            assemblies.AddRange(CompilationPipeline.GetAssemblies(AssembliesType.Editor));

            List<AssemblyInfo> outAssemblies = new();
            IReadOnlyList<FilterInfo> filters = config.AssemblyFilters;
            foreach (Assembly assembly in assemblies)
            {
                string assemblyName = assembly.name;

                for (int i = 0; i < filters.Count; i++)
                {
                    FilterInfo filter = filters[i];
                    
                    if (!filter.enabled)
                    {
                        continue;
                    }

                    if (!filter.filter.IsMatch(assemblyName))
                    {
                        continue;
                    }

                    outAssemblies.Add(new AssemblyInfo
                    {
                        name = assemblyName,
                        folders = GetAssemblyFolders(assembly),
                        regexId = i
                    });
                }
            }

            List<string> buffer = new();
            foreach (HashSet<string> folders in outAssemblies.Select(info => info.folders))
            {
                buffer.Clear();
                buffer.AddRange(folders);
                folders.Clear();
                foreach (string path in buffer)
                {
                    folders.Add(path.Replace('\\', '/'));
                }
            }

            return outAssemblies;
        }

        private HashSet<string> GetAssemblyFolders(Assembly assembly)
        {
            HashSet<string> outFolders = new();
            
            foreach (string file in assembly.sourceFiles)
            {
                string folder = GetDirectoryName(file);

                while (!(folder is null))
                {
                    outFolders.Add(folder);
                    folder = GetParentFolder(folder);
                }
            }

            return outFolders;
        }

        public string AssemblyToFileName(string assemblyName)
        {
            return assemblyName + ".csproj.DotSettings";
        }
        
        private string GetParentFolder(string path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (path == string.Empty) throw new ArgumentException($"{nameof(path)} can not be empty");
            
            int index = path.LastIndexOf('/');
            if (index == -1)
            {
                return null;
            }

            return path.Substring(0, index);
        }

        private string GetDirectoryName(string path)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (path == string.Empty) throw new ArgumentException($"{nameof(path)} can not be empty");
            
            int index = path.LastIndexOf('.');
            if (index == -1)
            {
                return path;
            }

            index = path.LastIndexOf('/');
            if (index == -1)
            {
                return path;
            }
            
            return path.Substring(0, index);
        }
    }
}