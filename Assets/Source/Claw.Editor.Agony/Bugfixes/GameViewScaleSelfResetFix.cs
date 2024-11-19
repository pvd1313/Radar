using System;
using System.Reflection;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Agony.Bugfixes
{
    /// <summary>
    /// Unity 2022.3.45
    /// Fixes Game View Editor Window scale reset after domain reloaded.
    /// </summary>
    [DomainInitialize]
    public static class GameViewScaleSelfResetFix
    {
        private const string PREFS_KEY_X = nameof(GameViewScaleSelfResetFix) + "_AAUO4ggU0JiK5i";
        private const string PREFS_KEY_Y = nameof(GameViewScaleSelfResetFix) + "_9iGSn8r4uhU0Am";

        private static readonly GameViewScaleSelfResetFixCash CASH;
        
        static GameViewScaleSelfResetFix()
        {
            // Bug seems to be not reproducing anymore. So disabled for now. 
            
            // try
            // {
            //     CASH = new GameViewReflection();
            //     
            //     AssemblyReloadEvents.afterAssemblyReload += _afterAssemblyReload;
            //     AssemblyReloadEvents.beforeAssemblyReload += _beforeAssemblyReload;
            // }
            // catch (Exception e)
            // {
            //     BDebug.StaticLogException(e);
            // }
        }

        private static void _afterAssemblyReload()
        {
            try
            {
                if (!PlayerPrefs.HasKey(PREFS_KEY_X) ||
                    !PlayerPrefs.HasKey(PREFS_KEY_Y))
                {
                    return;
                }
                
                Vector2 scale;
                scale.x = PlayerPrefs.GetFloat(PREFS_KEY_X);
                scale.y = PlayerPrefs.GetFloat(PREFS_KEY_Y);
                
                PlayerPrefs.DeleteKey(PREFS_KEY_X);
                PlayerPrefs.DeleteKey(PREFS_KEY_Y);
                
                EditorWindow window = CASH.Get_PlayModeWindow();
                FieldInfo zoomAreaField = CASH.GetField_PlayModeWindow_ZoomArea();
                object zoomArea = zoomAreaField.GetValue(window);

                FieldInfo scaleField = CASH.GetField_ZoomableArea_Scale();
                scaleField.SetValue(zoomArea, scale);
                
                // BDebug.StaticLogInfo("Applied scale: " + scale);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        private static void _beforeAssemblyReload()
        {
            try
            {
                EditorWindow window = CASH.Get_PlayModeWindow();
                FieldInfo zoomAreaField = CASH.GetField_PlayModeWindow_ZoomArea();
                object zoomArea = zoomAreaField.GetValue(window);

                FieldInfo scaleField = CASH.GetField_ZoomableArea_Scale();
                Vector2 scale = (Vector2)scaleField.GetValue(zoomArea);

                PlayerPrefs.SetFloat(PREFS_KEY_X, scale.x);
                PlayerPrefs.SetFloat(PREFS_KEY_Y, scale.y);

                // BDebug.StaticLogInfo("Saved current scale: " + scale);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
    }
}