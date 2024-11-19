using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Beast.Claw.Editor.Agony.Features
{
    [InitializeOnLoad]
    public static class ReplaceProjectBrowserTitleWithPath
    {
        private const int MAX_TITLE_LENGTH = 18;
        private const float UPDATE_RATE_SEC = 1;

        private static readonly Type PROJECT_BROWSER_TYPE;
        private static readonly MethodInfo PROJECT_BROWSER_GET_ACTIVE_FOLDER_PATH;
        private static readonly Texture ICON;
        private static readonly Dictionary<string, string> PATH_TO_TITLE_CASH;
        private static readonly Dictionary<Object, string> WINDOW_LAST_PATH;
        private static readonly HashSet<Object> KNOWN_WINDOWS;
        private static readonly StringBuilder STRING_BUILDER;

        private static DateTime _lastUpdateTime;
        
        static ReplaceProjectBrowserTitleWithPath()
        {
            try
            {
                PROJECT_BROWSER_TYPE = "UnityEditor.ProjectBrowser".TypeFromAssembly<Help>();

                PROJECT_BROWSER_GET_ACTIVE_FOLDER_PATH = "GetActiveFolderPath"
                    .MethodFromType(PROJECT_BROWSER_TYPE, Binding.INSTANCE_PUBLIC_PRIVATE);
                
                WINDOW_LAST_PATH = new Dictionary<Object, string>();
                PATH_TO_TITLE_CASH = new Dictionary<string, string>();
                KNOWN_WINDOWS = new HashSet<Object>();
                STRING_BUILDER = new StringBuilder();
                ICON = EditorGUIUtility.FindTexture("Project");

                EditorApplication.update += OnApplicationUpdate;
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        private static void OnApplicationUpdate()
        {
            try
            {
                DateTime now = DateTime.Now;

                if (now.GetTimePassedFrom(_lastUpdateTime).TotalSeconds < UPDATE_RATE_SEC)
                {
                    return;
                }

                _lastUpdateTime = now;
                
                Object[] allWindows = UnityEngine.Resources.FindObjectsOfTypeAll(PROJECT_BROWSER_TYPE);
                
                foreach (Object windowObject in allWindows)
                {
                    // Window may not be initialized yet. This hack fixes it.
                    if (KNOWN_WINDOWS.Add(windowObject))
                    {
                        continue;
                    }
                    
                    string path = (string) PROJECT_BROWSER_GET_ACTIVE_FOLDER_PATH.Invoke(windowObject, null);

                    if (WINDOW_LAST_PATH.TryGetValue(windowObject, out string lastPath) && (lastPath == path))
                    {
                        continue;
                    }

                    WINDOW_LAST_PATH[windowObject] = path;
                    string title = _pathToTitle(path);

                    EditorWindow windowEditor = (EditorWindow) windowObject;
                    windowEditor.titleContent = new GUIContent(title, ICON, path);
                    windowEditor.Repaint();
                }
            }
            catch (Exception e)
            {
                BDebug.LogException(e);

                EditorApplication.update -= OnApplicationUpdate;
            }
        }

        private static string _pathToTitle(string path)
        {
            if (PATH_TO_TITLE_CASH.TryGetValue(path, out string title))
            {
                return title;
            }
            
            string[] folders = path.Split('/');

            STRING_BUILDER.Clear();
            
            int counter = 0;
            int i = folders.Length - 1;
            
            for (; i >= 0 && (counter < MAX_TITLE_LENGTH); i--)
            {
                STRING_BUILDER.Insert(0, folders[i]);
                if (i > 0)
                {
                    STRING_BUILDER.Insert(0, '/');
                    counter++;
                }
                
                counter += folders[i].Length;
            }

            if (i > -1)
            {
                STRING_BUILDER.Insert(0, "..");
            }

            title = STRING_BUILDER.ToString();
            PATH_TO_TITLE_CASH[path] = title;

            return title;
        }
    }
}