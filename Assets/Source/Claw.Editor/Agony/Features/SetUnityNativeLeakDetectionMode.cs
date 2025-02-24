using System;
using Beast.Claw.Debug;
using Unity.Collections;
using UnityEditor;

namespace Beast.Claw.Editor.Agony.Features
{
    [InitializeOnLoad]
    public class SetUnityNativeLeakDetectionMode
    {
        static SetUnityNativeLeakDetectionMode()
        {
            try
            {
                NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
    }
}