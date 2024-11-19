using System;
using System.Reflection;
using Beast.Claw.Reflection;
using UnityEditor;

namespace Beast.Claw.Editor.Agony.Bugfixes
{
    public class GameViewScaleSelfResetFixCash
    {
        private const BindingFlags INSTANCE_PRIVATE = BindingFlags.Instance | BindingFlags.NonPublic; 

        private Type _playModeWindowType;
        private Type _zoomableAreaType;
        private EditorWindow _playModeWindow;
        private FieldInfo _playModeWindowZoomAreaType;
        private FieldInfo _zoomableAreaScale;

        public FieldInfo GetField_ZoomableArea_Scale()
        {
            BReflect.TypeFromAssembly<EditorApplication>("UnityEditor.ZoomableArea", ref _zoomableAreaType);
            BReflect.GetField(_zoomableAreaType, "m_Scale", INSTANCE_PRIVATE, ref _zoomableAreaScale);

            return _zoomableAreaScale;
        }
        
        public FieldInfo GetField_PlayModeWindow_ZoomArea()
        {
            BReflect.TypeFromAssembly<EditorApplication>("UnityEditor.GameView", ref _playModeWindowType);
            BReflect.GetField(_playModeWindowType, "m_ZoomArea", INSTANCE_PRIVATE, ref _playModeWindowZoomAreaType);

            return _playModeWindowZoomAreaType;
        }
        
        public EditorWindow Get_PlayModeWindow()
        {
            BReflect.TypeFromAssembly<EditorApplication>("UnityEditor.GameView", ref _playModeWindowType);
            BReflect.FindInternalUnityEditorWindow(_playModeWindowType, ref _playModeWindow);

            return _playModeWindow;
        }
    }
}