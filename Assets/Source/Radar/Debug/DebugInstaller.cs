using System;
using System.Collections.Generic;
using Beast.Claw.Common;
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
            yield return typeof(DebugPlayerPreferencesExamplePlayerTimeRecorderThatShouldBeEliminatedAsSoonAsPossible);
#endif

            yield break;
        }

        protected override void BindCustom(DiContainer diContainer)
        {
#if DEVELOPMENT
            diContainer.Bind<DebugPreferences>().FromNew().AsSingle().NonLazy();
#endif
        }
    }
}