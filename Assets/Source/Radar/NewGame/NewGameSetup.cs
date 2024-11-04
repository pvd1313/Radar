using System;
using Beast.Claw;
using Beast.Radar.Cinematic;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Unity.Mathematics;

namespace Beast.Radar.NewGame
{
    public class NewGameSetup : INewGameSetup
    {
        private readonly ISaveManager _saveManager;
        private readonly IWorldManager _worldManager;

        public NewGameSetup(ISaveManager saveManager, IWorldManager worldManager)
        {
            _saveManager = saveManager;
            _worldManager = worldManager;
        }
        
        public void StartNewGame(TerrainData terrain)
        {
            terrain.ValidateThrowException(nameof(terrain));
            
            _worldManager.Clean();
            
            WorldData world = new ()
            {
                guid = Guid.NewGuid(),
                terrain = terrain
            };
            
            _worldManager.AddWorld(world);

            GameModel model = new ()
            {
                camera = new CameraState
                {
                    position = terrain.terrainSize * 0.5f,
                    zoom01 = 1
                },
                
                player = new PlayerGameData()
                {
                    position = int2.zero
                },
                
                world = world.guid
            };

            _saveManager.LoadAsync(model);
        }
    }
}