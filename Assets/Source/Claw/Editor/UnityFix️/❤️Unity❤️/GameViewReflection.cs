using System;
using System.Reflection;
using Beast.Claw.Reflection;
using UnityEditor;

namespace Beast.Claw.Editor
{
    public class GameViewReflection
    {
        private const BindingFlags INSTANCE_PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic; 

        private Type _type_PlayModeWindow;
        private Type _type_ZoomableArea;
        private EditorWindow _window_PlayModeWindow;
        private FieldInfo _field_PlayModeWindow_zoomArea;
        private FieldInfo _field_ZoomableArea_Scale;

        public FieldInfo GetField_ZoomableArea_Scale()
        {
            ReflectionCash.TypeFromAssembly<EditorApplication>("UnityEditor.ZoomableArea", ref _type_ZoomableArea);
            ReflectionCash.GetField(_type_ZoomableArea, "m_Scale", INSTANCE_PRIVATE, ref _field_ZoomableArea_Scale);

            return _field_ZoomableArea_Scale;
        }
        
        public FieldInfo GetField_PlayModeWindow_ZoomArea()
        {
            ReflectionCash.TypeFromAssembly<EditorApplication>("UnityEditor.GameView", ref _type_PlayModeWindow);
            ReflectionCash.GetField(_type_PlayModeWindow, "m_ZoomArea", INSTANCE_PRIVATE, ref _field_PlayModeWindow_zoomArea);

            return _field_PlayModeWindow_zoomArea;
        }
        
        public EditorWindow Get_PlayModeWindow()
        {
            ReflectionCash.TypeFromAssembly<EditorApplication>("UnityEditor.GameView", ref _type_PlayModeWindow);
            ReflectionCash.GetUnityEditorWindow(_type_PlayModeWindow, ref _window_PlayModeWindow);

            return _window_PlayModeWindow;
        }
    }
}