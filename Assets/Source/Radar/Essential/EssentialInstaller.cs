using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Input;
using Beast.Claw.Reflection;
using Beast.Claw.Resources;
using Beast.Radar.Cinematic;
using Beast.Radar.Graphic;
using Beast.Radar.UI;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace Beast.Radar.Essential
{
    public class EssentialInstaller : ScriptableInstaller
    {
        [SerializeField] private EssentialConfig _essentialConfig;

        protected override IEnumerable<IConfig> BindConfigs()
        {
            yield return _essentialConfig;
        }

        protected override IEnumerable<System.Type> BindInterfaces()
        {
            yield return typeof(ResourceManager);
            
            // Camera
            yield return typeof(DeviceScreen);
            yield return typeof(CameraSpawner);
            
            // Input
            yield return typeof(InputManager);
            
            if (Application.isMobilePlatform || UnityEditorSimulatorWindow.HasFocus())
            {
                yield return typeof(TouchDevice);
            }
            else
            {
                yield return typeof(MouseDevice);
            }
            
            // UI
            yield return typeof(WindowManager);
            yield return typeof(NoesisWindowManager);
            
            yield return typeof(LoadingWindow);
            
            yield return typeof(EssentialBoot);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<Claw.GameCycle.GameCycle>().FromNew().AsSingle().NonLazy();
            diContainer.BindInterfacesAndSelfTo<Claw.GameCycle.GameCycle.Proxy>().FromNew().AsSingle().NonLazy();
        }
    }
}