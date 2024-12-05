using System;
using System.Collections.Generic;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Gizmo;
using Beast.Claw.Graphy;
using Beast.Radar.Cinematic;
using Beast.Radar.City;
using Beast.Radar.GameTime;
using Beast.Radar.NewGame;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Beast.Radar.UI;
using Beast.Radar.World;

namespace Beast.Radar.Core
{
    public class CoreInstaller : ScriptableInstaller
    {
        protected override IEnumerable<IConfig> _bindConfigs()
        {
            yield break;
        }

        protected override IEnumerable<Type> _bindInterfaces()
        {
            // Camera
            yield return typeof(CameraTransform);
            
            yield return typeof(GizmoRender);
            
            yield return typeof(CameraTransformController);
            
            // Main
            yield return typeof(CoreBoot);
            yield return typeof(SaveManager);
            yield return typeof(FileManager);
            yield return typeof(NewGameSetup);
            yield return typeof(WorldManager);
            yield return typeof(RadarGraphyManager);

            // Windows
            yield return typeof(MainHUDWindow);
            yield return typeof(PauseMenuWindow);
            yield return typeof(MapGenerationWindow);
            yield return typeof(GraphyWindowNoesis);
            
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
            yield return typeof(PlayerPreferenceSerializer);
            yield return typeof(GameTime.GameTime);
            yield return typeof(Player.Player);
            yield return typeof(GameTimeScale);

            // O_o
            yield return typeof(WorldViewInstance);
            yield return typeof(PlayerViewController);
            // yield return typeof(CityViewController);
        }

        protected override IEnumerable<Type> _bindModels()
        {
            yield return typeof(PlayerPreference);
            yield return typeof(GameModel);
        }
    }
}