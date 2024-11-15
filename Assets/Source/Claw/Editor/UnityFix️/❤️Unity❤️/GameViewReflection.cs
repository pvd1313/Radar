using System;
using System.Reflection;
using Beast.Claw.Reflection;
using UnityEditor;

namespace Beast.Claw.Editor
{
    public class GameViewReflection
    {
        private const BindingFlags INSTANCE_PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic; 

        private Type _playModeWindowType;
        private Type _zoomableAreaType;
        private EditorWindow _playModeWindow;
        private FieldInfo _playModeWindowZoomAreaType;
        private FieldInfo _zoomableAreaScale;

        public FieldInfo GetField_ZoomableArea_Scale()
        {
            ReflectionCash.TypeFromAssembly<EditorApplication>("UnityEditor.ZoomableArea", ref _zoomableAreaType);
            ReflectionCash.GetField(_zoomableAreaType, "m_Scale", INSTANCE_PRIVATE, ref _zoomableAreaScale);

            return _zoomableAreaScale;
        }
        
        public FieldInfo GetField_PlayModeWindow_ZoomArea()
        {
            ReflectionCash.TypeFromAssembly<EditorApplication>("UnityEditor.GameView", ref _playModeWindowType);
            ReflectionCash.GetField(_playModeWindowType, "m_ZoomArea", INSTANCE_PRIVATE, ref _playModeWindowZoomAreaType);

            return _playModeWindowZoomAreaType;
        }
        
        public EditorWindow Get_PlayModeWindow()
        {
            ReflectionCash.TypeFromAssembly<EditorApplication>("UnityEditor.GameView", ref _playModeWindowType);
            ReflectionCash.FindInternalUnityEditorWindow(_playModeWindowType, ref _playModeWindow);

            return _playModeWindow;
        }
    }
}