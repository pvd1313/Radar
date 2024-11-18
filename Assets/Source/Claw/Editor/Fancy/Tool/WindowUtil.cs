using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Fancy
{
    internal static class WindowUtil
    {
        private const int WINDOW_WIDTH = 200;
        private const int WINDOW_HEIGHT = 300;
        
        private static readonly HashSet<EditorWindow> NON_INITIALIZED_WINDOWS;

        static WindowUtil()
        {
            NON_INITIALIZED_WINDOWS = new HashSet<EditorWindow>();
        }
        
        public static void OpenToolWindow<T>() where T : EditorWindow
        {
            T window = ScriptableObject.CreateInstance<T>();
            NON_INITIALIZED_WINDOWS.Add(window);
            window.ShowPopup();
        }

        public static void InitializeWindowPosition(EditorWindow window)
        {
            const int WIDTH_HALF = WINDOW_WIDTH / 2;
            const int HEIGHT_HALF = WINDOW_HEIGHT / 2;
            
            Event currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type == EventType.Ignore)
            {
                return;
            }

            if (NON_INITIALIZED_WINDOWS.Remove(window))
            {
                Vector2 pos = currentEvent.mousePosition;
                window.position = new Rect(pos.x - WIDTH_HALF, pos.y - HEIGHT_HALF, WINDOW_WIDTH, WINDOW_HEIGHT);
            }
        }
    }
}