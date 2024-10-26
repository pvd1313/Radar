using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Core
{
    public class CoreBoot : ICoreBoot
    {
        private readonly IWindowManager _window;
        private readonly ISaveManager _save;
        private readonly IntegratedSave _integratedSave;
        private readonly IGameCycle _gameCycle;

        public CoreBoot(
            IWindowManager window, 
            ISaveManager save, 
            IntegratedSave integratedSave, 
            IGameCycle gameCycle)
        {
            _window = window;
            _save = save;
            _integratedSave = integratedSave;
            _gameCycle = gameCycle;
        }
        
        public async UniTask<OperationResult> Boot()
        {
            // TODO: Start displaying loading infos.
            //     : https://risenbeast.youtrack.cloud/issue/R-78/Loading-progress-in-ui-screen
            _window.Show(WindowID.LoadingScreen);
            
            _gameCycle.RegisterListeners();

            await _gameCycle.Execute(this, OperationID.PREFERENCES_LOAD);

            await _gameCycle.Execute(this, OperationID.WARMUP);

            BScene.CreateGameObject(this)
                .AddComponent<GameCycleMono>()
                .Initialize(_gameCycle);
            
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
                    OperationResult result = await _save.Load(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);

                    if (result.IsSuccess())
                    {
                        _window.Show(WindowID.MainHUD);
                    }
                    else
                    {
                        _window.Show(WindowID.MapGeneration);
                    }
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