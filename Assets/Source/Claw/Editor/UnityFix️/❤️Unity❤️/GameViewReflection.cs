using System;
using System.Reflection;
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
            _cashedType<EditorApplication>("UnityEditor.ZoomableArea", ref _type_ZoomableArea);
            _cashedField(_type_ZoomableArea, "m_Scale", INSTANCE_PRIVATE, ref _field_ZoomableArea_Scale);

            return _field_ZoomableArea_Scale;
        }
        
        public FieldInfo GetField_PlayModeWindow_ZoomArea()
        {
            _cashedType<EditorApplication>("UnityEditor.GameView", ref _type_PlayModeWindow);
            
            _cashedField(_type_PlayModeWindow, "m_ZoomArea", INSTANCE_PRIVATE, ref _field_PlayModeWindow_zoomArea);

            return _field_PlayModeWindow_zoomArea;
        }
        
        public EditorWindow Get_PlayModeWindow()
        {
            _cashedType<EditorApplication>("UnityEditor.GameView", ref _type_PlayModeWindow);
            
            _cashedWindow(_type_PlayModeWindow, ref _window_PlayModeWindow);

            return _window_PlayModeWindow;
        }

        private void _cashedField(Type ownerType, string fieldName, BindingFlags flags, ref FieldInfo cash)
        {
            if (cash != null)
            {
                return;
            }

            cash = ownerType.GetField(fieldName, flags);

            if (cash == null)
            {
                throw new Exception($"Failed to get field {ownerType} ({fieldName})");
            }
        }

        private void _cashedWindow(Type windowType, ref EditorWindow cash)
        {
            if (cash != null)
            {
                return;
            }

            cash = EditorWindow.GetWindow(windowType);
            
            if (cash == null)
            {
                throw new Exception($"Failed: EditorWindow.GetWindow({windowType.FullName})");
            }
        }
        
        private void _cashedType<TAssembly>(string name, ref Type cash)
        {
            if (cash != null)
            {
                return;
            }
            
            cash = typeof(TAssembly).Assembly.GetType(name, true);
        }
    }
}