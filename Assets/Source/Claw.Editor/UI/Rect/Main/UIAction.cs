// #define UI_ACTION_STACK_TRACE

using System;
using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public class UIAction : IDisposable
    {
        private enum EAction
        {
            None,
            DisableInteraction,
            RemoveIndent,
        }
        
        
#if UI_ACTION_STACK_TRACE
        private static readonly bool STACK_TRACING = true;
#else
        private static readonly bool STACK_TRACING = false;
#endif
        
        private int _value;
        private EAction _action;
        private string _stackTrace;

        public UIAction RemoveIndent()
        {
            _leakCheck();
            
            _action = EAction.RemoveIndent;
            _value = EditorGUI.indentLevel;

            EditorGUI.indentLevel = 0;

            return this;
        }

        public UIAction RemoveInteraction()
        {
            _leakCheck();

            _action = EAction.DisableInteraction;
            _value = GUI.enabled ? 1 : 0;

            GUI.enabled = false;

            return this;
        }

        private void _leakCheck()
        {
            if (_action == EAction.None)
            {
                if (STACK_TRACING)
                {
                    _stackTrace = Environment.StackTrace;
                }
                
                return;
            }
            
            string message = $"Detected action that was not disposed: '{_action}'. ";
                
            if (STACK_TRACING)
            {
                message += $"StackTrace: {Environment.NewLine}" + _stackTrace + Environment.NewLine;
            }
            else
            {
                message += "Enable stack tracing to detect leak.";
            }
                
            BDebug.LogWarning(this, message);
        }
        
        public void Dispose()
        {
            if (_action != EAction.None)
            {
                switch (_action)
                {
                    case EAction.RemoveIndent:
                        EditorGUI.indentLevel = _value;
                        break;
                    
                    case EAction.DisableInteraction:
                        GUI.enabled = _value == 1;
                        break;
                }

                _action = EAction.None;
            }
        }
    }
}