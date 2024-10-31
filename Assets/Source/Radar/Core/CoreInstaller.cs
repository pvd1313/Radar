using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Radar.Cinematic;
using Beast.Radar.InputHandling;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Beast.Radar.UI;
using Beast.Radar.World;
using Zenject;

namespace Beast.Radar.Core
{
    public class CoreInstaller : ScriptableInstaller
    {
        protected override IEnumerable<IConfig> BindConfigs()
        {
            yield break;
        }

        protected override IEnumerable<Type> BindInterfaces()
        {
            // Main
            yield return typeof(CoreBoot);
            yield return typeof(SaveManager);
            yield return typeof(FileManager);
            yield return typeof(NewGameSetup);
            
            // Input & Camera
            yield return typeof(InputSystem);
            yield return typeof(CameraTransform);
            yield return typeof(CameraController);
            
            // UI
            yield return typeof(InputBlocker);
            yield return typeof(SelectableZoneBuilder);

            // UI Windows
            yield return typeof(HUDWindow);
            yield return typeof(PauseMenuWindow);
            yield return typeof(MapGenerationWindow);
            
            // Terrain
            yield return typeof(TerrainFbmRender);
            yield return typeof(TerrainBiomeRender);
            yield return typeof(TerrainBakedFbm);
            yield return typeof(TerrainColorRender);
            yield return typeof(TerrainTileRender);
            yield return typeof(TerrainPreviewRender);
            yield return typeof(TerrainBakedBiome);
            yield return typeof(TerrainTest);
            
            //Player
            yield return typeof(PlayerState);
            yield return typeof(WorldTime);
            yield return typeof(PlayerMovement);
            yield return typeof(PlayerViewController);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<GameModel>().FromNew().AsSingle().NonLazy();
        }
    }
}