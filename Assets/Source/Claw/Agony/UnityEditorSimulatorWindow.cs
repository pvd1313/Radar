using System;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;

namespace Beast.Claw.Agony
{
    public class UnityEditorSimulatorWindow
#if UNITY_EDITOR
    {
        public bool CanFocus => !_isBroken;
        
        private readonly Type _windowType;
        
        private UnityEditor.EditorWindow _simulatorWindow;

        private bool _isBroken;

        public UnityEditorSimulatorWindow()
        {
            try
            {
                _windowType = "UnityEditor.DeviceSimulation.SimulatorWindow"
                    .TypeFromAssembly<UnityEditor.DeviceSimulation.DeviceSimulator>();
            }
            catch (Exception e)
            {
                _isBroken = true;
                
                BDebug.LogException(e);
            }
        }
        
        // Executed only in UnityEditor.
        // ReSharper disable Unity.PerformanceAnalysis
        public bool CheckHasFocus()
        {
            if (_isBroken)
            {
                return false;
            }

            try
            {
                if (BReflect.TryFindInternalUnityEditorWindow(_windowType, ref _simulatorWindow))
                {
                    return _simulatorWindow.hasFocus;
                }
            }
            catch (Exception e)
            {
                _isBroken = true;
                
                BDebug.LogException(e);
            }
            
            return false;
        }
    }
#else
    {
        public bool CheckHasFocus() => false;

        public bool CanFocus => false;
    }
#endif
}