using System;
using Beast.Claw.Common;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial struct BeastEditorUI
    {
        public bool success;
        public bool niceLabelsEnabled;
        
        private string NicifyLabel(string label)
        {
            if (!niceLabelsEnabled)
            {
                return label;
            }
            
            return ObjectNames.NicifyVariableName(label);
        }

        private BeastEditorUI ExecuteActions(params Action[] actions)
        {
            bool isSuccess = true;
            
            foreach (Action action in actions)
            {
                TryCatch(action, out bool actionSuccess);

                if (!actionSuccess)
                {
                    isSuccess = false;
                }
            }

            return new BeastEditorUI
            {
                success = isSuccess
            };
        }
        
        public void TryCatch(Action action, out bool actionSuccess)
        {
            actionSuccess = true;

            try
            {
                action();
            }

            catch (ExitGUIException)
            {
                
            }
            
            catch (BeastAssertionException e)
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

                BLog.StaticException(e, "BeastEditorWindow unhandled exception");
            }
        }
    }
}