using Beast.Claw;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Beast.Radar.World;
using Unity.Mathematics;

namespace Beast.Radar.Core
{
    public class NewGameSetup : INewGameSetup
    {
        private readonly ISaveManager _saveManager;

        public NewGameSetup(ISaveManager saveManager)
        {
            _saveManager = saveManager;
        }
        
        public void StartNewGame(TerrainData terrain)
        {
            terrain.ValidateThrowException(nameof(terrain));

            GameModel model = new ()
            {
                terrain = terrain,
                camera = new CameraState
                {
                    position = terrain.terrainSize.Multiply(0.5f),
                    zoom01 = 1
                },
                player = new PlayerGameData
                {
                    position = int2.zero,
                },
                world = new WorldData()
            };

            _saveManager.LoadAsync(model);
        }
    }
}