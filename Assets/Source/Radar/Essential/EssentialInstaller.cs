using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Resources;
using Beast.Radar.Cinematic;
using Beast.Radar.Graphic;
using Beast.Radar.UI;
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

        protected override IEnumerable<Type> BindInterfaces()
        {
            yield return typeof(CameraSpawner);
            yield return typeof(DeviceScreen);
            yield return typeof(WindowManager);
            yield return typeof(LoadingScreenWindow);
            yield return typeof(EssentialBoot);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<Claw.GameCycle.GameCycle>().FromNew().AsSingle().NonLazy();
            diContainer.BindInterfacesAndSelfTo<Claw.GameCycle.GameCycle.Proxy>().FromNew().AsSingle().NonLazy();
        }
    }
}