using System;
using Beast.Claw.Attributes;
using Beast.Claw.GameLoop;
using Beast.Claw.Validation;
using Beast.Radar.Cinematic;
using Beast.Radar.Player;
using Beast.Radar.Serialization;
using Beast.Radar.Terrain;
using Beast.Radar.UI;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.NewGame
{
    public class NewGameSetup : INewGameSetup
    {
        [Required] private readonly ISaveManager _saveManager;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly IWindowManager _windowManager;
        
        public async UniTask StartNewGame(TerrainData terrain)
        {
            terrain.ValidateThrowException(nameof(terrain));
            
            _worldManager.Clean();
            
            WorldData world = new()
            {
                guid = Guid.NewGuid(),
                terrain = terrain
            };
            
            _worldManager.AddWorld(world);

            GameModel model = new()
            {
                camera = new CameraState
                {
                    position = terrain.terrainSize * 0.5f,
                    zoom01 = 1
                },
                
                player = new PlayerGameData
                {
                    position = int2.zero
                },
                
                world = world.guid
            };

            EActionStatus result = await _saveManager.LoadAsync(model, ESaveManagerFlags.None);

            if (result == EActionStatus.Success)
            {
                _saveManager.Save(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
            }
        }
    }
}