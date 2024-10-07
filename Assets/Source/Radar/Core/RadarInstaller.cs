using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Beast.Radar.Debug;
using Beast.Radar.GameCycle;
using Beast.Radar.Graphic;
using Beast.Radar.InputControl;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Beast.Radar.World;
using Zenject;
using Installer = Beast.Claw.Common.Installer;

namespace Beast.Radar.Core
{
    public class RadarInstaller : Installer
    {
        protected override IEnumerable<Type> BindInterfaces()
        {
            // Main
            yield return typeof(RadarGameCycle);
            yield return typeof(RadarGameBoot);
            yield return typeof(GameSaveManager);
            
            // UI Main
            yield return typeof(UIFactory);
            yield return typeof(WindowManager);
            yield return typeof(InputBlocker);
            
            // UI Windows
            yield return typeof(HUDWindow);
            yield return typeof(PauseMenuWindow);
            yield return typeof(MapGenerationWindow);
            
            // Input & Camera
            yield return typeof(PlayerInputProvider);
            yield return typeof(UIInputController);
            yield return typeof(CameraInputController);
            yield return typeof(CameraController);
            
            // Terrain
            yield return typeof(TerrainBaker);
            yield return typeof(WorldMapRender);
            yield return typeof(TerrainTexture);
            
            // Debug
            yield return typeof(DebugController);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<Claw.GameCycle.GameCycle>().FromNew().AsSingle().NonLazy();
            diContainer.BindInterfacesAndSelfTo<GameCycleProxy>().FromNew().AsSingle().NonLazy();
            diContainer.Bind<GameModel>().FromNew().AsSingle().NonLazy();
        }
    }
}