using Beast.Claw;
using Beast.Claw.Mathematics;
using Beast.Radar.Cinematic;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;

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
            terrain.ValidateThrowException();

            GameModel model = new ()
            {
                terrain = terrain,
                camera = new CameraState
                {
                    position = terrain.terrainSize.Multiply(0.5f),
                    zoom01 = 1
                }
            };

            _saveManager.Load(model);
        }
    }
}