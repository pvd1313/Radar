using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Beast.Radar.Debug;
using Beast.Radar.InputControl;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Beast.Radar.World;
using Zenject;

namespace Beast.Radar.Core
{
    public class RadarInstaller : BeastInstaller
    {
        protected override IEnumerable<Type> BindInterfaces()
        {
            // Main
            yield return typeof(RadarGameCycle);
            yield return typeof(RadarGameBoot);
            yield return typeof(BeastGameSaveManager);
            
            // UI Main
            yield return typeof(BeastUIFactory);
            yield return typeof(BeastWindowManager);
            yield return typeof(BeastInputBlocker);
            
            // UI Windows
            yield return typeof(BeastHUDWindow);
            yield return typeof(BeastPauseWindow);
            yield return typeof(BeastMapGenerationWindow);
            
            // Input & Camera
            yield return typeof(BeastPlayerInputProvider);
            yield return typeof(BeastUIInputController);
            yield return typeof(BeastCameraInputController);
            yield return typeof(BeastCameraController);
            
            // Terrain
            yield return typeof(BeastTerrainBaker);
            yield return typeof(WorldMapRender);
            yield return typeof(BeastTerrainTexture);
            
            // Debug
            yield return typeof(BeastDebugController);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<ClawGameCycle>().FromNew().AsSingle().NonLazy();
            diContainer.BindInterfacesAndSelfTo<ClawGameCycleProxy>().FromNew().AsSingle().NonLazy();
            diContainer.Bind<BeastGameModel>().FromNew().AsSingle().NonLazy();
        }
    }
}