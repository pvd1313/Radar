using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Cinematic;
using Beast.Radar.GameCycle;
using Beast.Radar.Graphic;
using Beast.Radar.Graphic.Terrain;
using Beast.Radar.InputControl;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Zenject;

namespace Beast.Radar.Core
{
    public class CoreInstaller : ScriptableInstaller
    {
        protected override IEnumerable<Type> BindInterfaces()
        {
            // Main
            yield return typeof(GameCycle.GameCycle);
            yield return typeof(GameBoot);
            yield return typeof(GameSaveManager);
            yield return typeof(DeviceScreen);
            yield return typeof(FileManager);
            
            // UI Main
            yield return typeof(UIFactory);
            yield return typeof(WindowManager);
            yield return typeof(InputBlocker);
            yield return typeof(SelectableZoneBuilder);

            // UI Windows
            yield return typeof(HUDWindow);
            yield return typeof(PauseMenuWindow);
            //yield return typeof(MapGenerationWindow);
            
            // Input & Camera
            yield return typeof(CameraTransform);
            yield return typeof(PlayerInputProvider);
            yield return typeof(UIInputController);
            yield return typeof(CameraInputController);
            yield return typeof(CameraController);
            
            // Render
            yield return typeof(TerrainBaker);
            yield return typeof(TerrainBiomeRender);
            yield return typeof(TerrainBakedTexture);
            yield return typeof(TerrainColorRender);
            yield return typeof(TerrainTileRender);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<Claw.GameCycle.GameCycle>().FromNew().AsSingle().NonLazy();
            diContainer.BindInterfacesAndSelfTo<GameCycleProxy>().FromNew().AsSingle().NonLazy();
            diContainer.Bind<GameModel>().FromNew().AsSingle().NonLazy();
        }
    }
}