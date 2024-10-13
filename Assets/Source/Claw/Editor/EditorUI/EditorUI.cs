using System;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial struct EditorUI
    {
        public bool success;
        public bool niceLabelsEnabled;
        public GUILayoutOption[] layoutOptions;
        
        private string NicifyLabel(string label)
        {
            if (!niceLabelsEnabled)
            {
                return label;
            }
            
            return ObjectNames.NicifyVariableName(label);
        }

        private EditorUI ExecuteActions(params Action<EditorUI>[] actions)
        {
            bool isSuccess = true;
            
            foreach (Action<EditorUI> action in actions)
            {
                TryCatch(action, out bool actionSuccess);

                if (!actionSuccess)
                {
                    isSuccess = false;
                }
            }

            return new EditorUI
            {
                success = isSuccess
            };
        }
        
        public void TryCatch(Action<EditorUI> action, out bool actionSuccess)
        {
            actionSuccess = true;

            try
            {
                action(this);
            }

            catch (ExitGUIException)
            {
                
            }
            
            catch (AssertionException e)
            {
                actionSuccess = false;
                
                EditorGUILayout.HelpBox(e.Message, MessageType.Warning);
            }
            
            catch (Exception e)
            {
                actionSuccess = false;
                
                EditorGUILayout.HelpBox($"Unhandled exception:", MessageType.Error);

                EditorGUILayout.LabelField(e.Message);
                
                EditorGUILayout.TextArea(e.StackTrace);

                BDebug.StaticLogException(e);
            }
        }
    }
}