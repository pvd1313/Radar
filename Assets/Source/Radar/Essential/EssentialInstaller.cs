using System;
using System.Collections.Generic;
using Beast.Claw.Asset;
using Beast.Claw.Common;
using Beast.Radar.Graphic;
using Beast.Radar.UI;
using Zenject;

namespace Beast.Radar.Essential
{
    public class EssentialInstaller : ScriptableInstaller
    {
        protected override IEnumerable<Type> BindInterfaces()
        {
            yield return typeof(DeviceScreen);
            yield return typeof(UIFactory);
            yield return typeof(WindowManager);
            yield return typeof(LoadingScreenWindow);
            yield return typeof(AssetManager);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<Claw.GameCycle.GameCycle>().FromNew().AsSingle().NonLazy();
            diContainer.BindInterfacesAndSelfTo<Claw.GameCycle.GameCycle.Proxy>().FromNew().AsSingle().NonLazy();
        }
    }
}