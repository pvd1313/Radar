using System;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    [FancyAsset]
    public abstract partial class BeastEditorWindow : EditorWindow
    {
        protected abstract void OnInspectorGUI();

        protected abstract void Initialize();

        protected abstract void Dispose();

        protected BeastEditorWindow()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void OnGUI()
        {
            TryCatch(OnInspectorGUI, out bool success);
        }

        protected BeastEditorUIResult Vertical(params Action[] actions)
        {
            EditorGUILayout.BeginVertical();

            BeastEditorUIResult result = ExecuteActions(actions);
            
            EditorGUILayout.EndVertical();

            return result;
        }

        protected BeastEditorUIResult Horizontal(params Action[] actions)
        {
            EditorGUILayout.BeginHorizontal();

            BeastEditorUIResult result = ExecuteActions(actions);
            
            EditorGUILayout.EndHorizontal();

            return result;
        }

        protected BeastEditorUIResult HorizontalEnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!isEnabled);
            
            BeastEditorUIResult result = ExecuteActions(actions);
            
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            return result;
        }

        protected BeastEditorUIResult VerticalEnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(!isEnabled);

            BeastEditorUIResult result = ExecuteActions();
            
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            return result;
        }

        protected BeastEditorUIResult EnabledGroup(bool isEnabled, params Action[] actions)
        {
            EditorGUI.BeginDisabledGroup(!isEnabled);
            
            BeastEditorUIResult result = ExecuteActions(actions);
            
            EditorGUI.EndDisabledGroup();

            return result;
        }

        private BeastEditorUIResult ExecuteActions(params Action[] actions)
        {
            bool isOK = true;
            
            foreach (Action action in actions)
            {
                TryCatch(action, out bool success);

                if (!success)
                {
                    isOK = false;
                }
            }

            return new BeastEditorUIResult
            {
                success = isOK
            };
        }
        
        private static void TryCatch(Action action, out bool success)
        {
            success = true;

            try
            {
                action();
            }

            catch (ExitGUIException)
            {
                
            }
            
            catch (BeastAssertionException e)
            {
                success = false;
                
                EditorGUILayout.HelpBox(e.Message, MessageType.Warning);
            }
            
            catch (Exception e)
            {
                success = false;
                
                EditorGUILayout.HelpBox($"Unhandled exception:", MessageType.Error);

                EditorGUILayout.LabelField(e.Message);
                
                EditorGUILayout.TextArea(e.StackTrace);

                BeastLog.StaticException(e, "BeastEditorWindow unhandled exception");
            }
        }
    }
}