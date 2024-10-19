using Beast.Claw.GameCycle;
using Beast.Radar.Core;
using Beast.Radar.Debug;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TerrainData = Beast.Radar.Terrain.TerrainData;

namespace Beast.Radar.GameCycle
{
    public class GameBoot : IGameBoot
    {
        private readonly IWindowManager _window;
        private readonly ISaveManager _save;
        private readonly IntegratedSave _integratedSave;

        public GameBoot(IWindowManager window, ISaveManager save, IntegratedSave integratedSave)
        {
            _window = window;
            _save = save;
            _integratedSave = integratedSave;
        }
        
        public async UniTask<OperationResult> Boot()
        {
            if (Application.isEditor)
            {
                await _save.Load(_integratedSave.model);
                
                _window.Show(WindowID.MainHUD);
            }
            else
            {
                bool saveExists = await _save.Exists(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
                
                if (saveExists)
                {
                    await _save.Load(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
                    
                    _window.Show(WindowID.MainHUD);
                }
                else
                {
                    _window.Show(WindowID.MapGeneration);
                }
            }

            return OperationResult.Success();
        }
    }
}