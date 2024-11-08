using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.Gizmo;
using Beast.Radar.Cinematic;
using Beast.Radar.City;
using Beast.Radar.GameTime;
using Beast.Radar.InputHandling;
using Beast.Radar.NewGame;
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
            // Input & Camera
            yield return typeof(InputSystem);
            yield return typeof(CameraTransform);
            yield return typeof(CameraController);
            
            // Main
            yield return typeof(CoreBoot);
            yield return typeof(SaveManager);
            yield return typeof(FileManager);
            yield return typeof(NewGameSetup);
            yield return typeof(WorldManager);
            yield return typeof(GizmoRender);
            
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
            yield return typeof(TerrainBiomeTest);
            yield return typeof(CitySpawner);
            
            // Player
            yield return typeof(PlayerPreferencesController);
            yield return typeof(GameTimeController);
            yield return typeof(PlayerController);
            yield return typeof(PlayerViewController);
            yield return typeof(WorldTimeStateController);
        }

        protected override void BindCustom(DiContainer diContainer)
        {
            diContainer.Bind<PlayerPreferences>().FromNew().AsSingle().NonLazy();
            diContainer.Bind<GameModel>().FromNew().AsSingle().NonLazy();
        }
    }
}