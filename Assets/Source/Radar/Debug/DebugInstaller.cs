using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Gizmo;
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
            if (Build.IS_DEVELOPMENT)
            {
                yield return typeof(GizmoRender);
                yield return typeof(DebugController);
                yield return typeof(DebugPreferencesController);
                yield return typeof(DebugMenuWindow);
            }
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            if (Build.IS_DEVELOPMENT)
            {
                diContainer.Bind<DebugPreferences>().FromNew().AsSingle().NonLazy();
            }
        }
    }
}