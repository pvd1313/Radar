using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.Core
{
    public class CoreBoot : Controller, ICoreBoot
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
        
        public async UniTask<ActionResult> Boot()
        {
            IWindowHandle<Empty> loadingScreen = _window.GetHandle<Empty>(this, WindowID.LoadingScreen);
            IWindowHandle<Empty> mainHUD = _window.GetHandle<Empty>(this, WindowID.MainHUD);
            IWindowHandle<Empty> mapGeneration = _window.GetHandle<Empty>(this, WindowID.MapGeneration);

            // TODO: Start displaying loading infos.
            //     : https://risenbeast.youtrack.cloud/issue/R-78/Loading-progress-in-ui-screen
            await loadingScreen.Open(default);
            
            _gameCycle.RegisterListeners();

            await _gameCycle.Execute(this, ActionID.PREFERENCES_LOAD);

            await _gameCycle.Execute(this, ActionID.WARMUP);

            BScene.CreateGameObject(this)
                .AddComponent<GameCycleMono>()
                .Initialize(_gameCycle);
            
            if (Application.isEditor)
            {
                await _save.Load(_integratedSave.model);

                await mainHUD.Open(default);
            }
            else
            {
                bool saveExists = await _save.Exists(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
                
                if (saveExists)
                {
                    ActionResult result = await _save.Load(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);

                    if (result.IsSuccess())
                    {
                        await mainHUD.Open(default);
                    }
                    else
                    {
                        await mapGeneration.Open(default);
                    }
                }
                else
                {
                    await mapGeneration.Open(default);
                }
            }

            return ActionResult.Success();
        }
    }
}