using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Input;
using Beast.Claw.Resources;
using Beast.Radar.Cinematic;
using Beast.Radar.Graphic;
using Beast.Radar.UI;
using UnityEngine;

namespace Beast.Radar.Essential
{
    public class EssentialInstaller : ScriptableInstaller
    {
        [SerializeField] private EssentialConfig _essentialConfig;

        protected override IEnumerable<IConfig> _bindConfigs()
        {
            yield return _essentialConfig;
        }

        protected override IEnumerable<Type> _bindInterfaces()
        {
            yield return typeof(GameLoop);
            
            yield return typeof(Runtime);
            yield return typeof(ResourceManager);
            yield return typeof(FrameTime);
            
            // Camera
            yield return typeof(DeviceScreen);
            yield return typeof(CameraSpawner);
            
            // Input
            yield return typeof(UnityInputDriver);
            yield return typeof(InputManager);
            yield return typeof(DeviceManager);
            
            // UI
            yield return typeof(WindowManager);
            yield return typeof(NoesisWindowManager);
            
            yield return typeof(LoadingWindow);
            
            yield return typeof(EssentialBoot);
        }

        protected override IEnumerable<Type> _bindModels()
        {
            yield break;
        }
    }
}