using System;
using Beast.Claw.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Beast.Claw.Reflection
{
    public static class UnityEditorSimulatorWindow
    {
#if UNITY_EDITOR
        private const string SIMULATOR_WINDOW = "UnityEditor.DeviceSimulation.SimulatorWindow";

        private static Type _simulatorWindowType;
        private static EditorWindow _simulatorWindow;
        private static Type _isActive;
#endif
        
        public static bool HasFocus()
        {
#if UNITY_EDITOR
            try
            {
                ReflectionCash.TypeFromAssembly<UnityEditor.DeviceSimulation.DeviceSimulator>(SIMULATOR_WINDOW, ref _simulatorWindowType);

                ReflectionCash.GetUnityEditorWindow(_simulatorWindowType, ref _simulatorWindow);

                return _simulatorWindow.hasFocus;
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
#endif
            return false;
        }
    }
}