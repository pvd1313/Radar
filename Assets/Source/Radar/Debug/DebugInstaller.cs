using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.UI;
using Zenject;

namespace Beast.Radar.Debug
{
    public class DebugInstaller : ScriptableInstaller
    {
        protected override IEnumerable<IConfig> BindConfigs()
        {
            yield break;
        }

        protected override IEnumerable<Type> BindInterfaces()
        {
            yield return typeof(DebugController);
            yield return typeof(DebugPreferencesController);
            yield return typeof(DebugMenuWindow);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<DebugPreferences>().FromNew().AsSingle().NonLazy();
        }
    }
}