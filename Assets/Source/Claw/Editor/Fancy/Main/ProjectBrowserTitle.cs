using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beast.Claw.Editor.Fancy.Main
{
    [InitializeOnLoad]
    public static class ProjectBrowserTitle
    {
        private const int MaxTitleLength = 18;

        private static readonly Type ProjectBrowserType;
        private static readonly MethodInfo ProjectBrowserGetActiveFolderPath;
        private static readonly Texture Icon;
        private static readonly Dictionary<string, string> PathToTitleCash;
        private static readonly Dictionary<Object, string> WindowLastPath;
        private static readonly HashSet<Object> KnownWindows;
        private static readonly StringBuilder StringBuilder;

        private static double _lastUpdateTime;
        
        static ProjectBrowserTitle()
        {
            try
            {
                Assembly editorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
                ProjectBrowserType = editorAssembly.GetType("UnityEditor.ProjectBrowser");
                if (ProjectBrowserType is null)
                {
                    BDebug.StaticLogError("Failed to resolve 'UnityEditor.ProjectBrowser'");
                    return;
                }

                if (!GetMethodInfo("GetActiveFolderPath", out ProjectBrowserGetActiveFolderPath))
                {
                    return;
                }

                WindowLastPath = new Dictionary<Object, string>();
                PathToTitleCash = new Dictionary<string, string>();
                KnownWindows = new HashSet<Object>();
                StringBuilder = new StringBuilder();
                Icon = EditorGUIUtility.FindTexture("Project");

                EditorApplication.update -= OnApplicationUpdate;
                EditorApplication.update += OnApplicationUpdate;
            }
            catch (Exception e)
            {
                BDebug.StaticLogException(e);
            }
        }

        private static bool GetMethodInfo(string methodName, out MethodInfo methodInfo)
        {
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
            
            methodInfo = ProjectBrowserType.GetMethod(methodName, flags);
            if (methodInfo is null)
            {
                BDebug.StaticLogError($"Failed to get method info 'ProjectBrowser.{methodName}()");
                return false;
            }

            return true;
        }

        private static void OnApplicationUpdate()
        {
            try
            {
                double time = EditorApplication.timeSinceStartup;
                if (_lastUpdateTime + 1 > EditorApplication.timeSinceStartup)
                {
                    return;
                }

                _lastUpdateTime = time;
                
                Object[] allWindows = Resources.FindObjectsOfTypeAll(ProjectBrowserType);
                foreach (Object windowObject in allWindows)
                {
                    // Window may not be initialized yet. This hack fixes it.
                    if (KnownWindows.Add(windowObject))
                    {
                        continue;
                    }
                    
                    string path = (string) ProjectBrowserGetActiveFolderPath.Invoke(windowObject, null);

                    if (WindowLastPath.TryGetValue(windowObject, out string lastPath) && (lastPath == path))
                    {
                        continue;
                    }

                    WindowLastPath[windowObject] = path;
                    string title = PathToTitle(path);

                    EditorWindow windowEditor = (EditorWindow) windowObject;
                    windowEditor.titleContent = new GUIContent(title, Icon, path);
                    windowEditor.Repaint();
                }
            }
            catch (Exception e)
            {
                BDebug.StaticLogException(e);

                EditorApplication.update -= OnApplicationUpdate;
            }
        }

        private static string PathToTitle(string path)
        {
            if (PathToTitleCash.TryGetValue(path, out string title))
            {
                return title;
            }
            
            string[] folders = path.Split('/');

            StringBuilder.Clear();
            
            int counter = 0;
            int i = folders.Length - 1;
            
            for (; i >= 0 && (counter < MaxTitleLength); i--)
            {
                StringBuilder.Insert(0, folders[i]);
                if (i > 0)
                {
                    StringBuilder.Insert(0, '/');
                    counter++;
                }
                
                counter += folders[i].Length;
            }

            if (i > -1)
            {
                StringBuilder.Insert(0, "..");
            }

            title = StringBuilder.ToString();
            PathToTitleCash[path] = title;

            return title;
        }
    }
}