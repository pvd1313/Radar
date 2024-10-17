using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.UI;
using Zenject;

namespace Beast.Radar.Debug
{
    public class DebugInstaller : ScriptableInstaller
    {
        protected override IEnumerable<Type> BindInterfaces()
        {
#if DEVELOPMENT
            yield return typeof(DebugController);
            yield return typeof(DebugPreferencesController);
            yield return typeof(DebugMenuWindow);
#endif
        }

        protected override void BindCustom(DiContainer diContainer)
        {
#if DEVELOPMENT
            diContainer.Bind<DebugPreferences>().FromNew().AsSingle().NonLazy();
#endif
        }
    }
}