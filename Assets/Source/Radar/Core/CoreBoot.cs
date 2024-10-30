using System;
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
        
        public async UniTask Boot(IProgressTracker tracker)
        {
            IWindowHandle<Empty> mainHUD = _window.GetHandle<Empty>(this, WindowID.MainHUD);
            IWindowHandle<Empty> mapGeneration = _window.GetHandle<Empty>(this, WindowID.MapGeneration);
            IWindowHandle<LoadingScreenInput> loadingScreen = _window.GetHandle<LoadingScreenInput>(this, WindowID.LoadingScreen);

            tracker.Begin(4, 1 - tracker.Progress);

            await tracker.Next("Core: Registering GameCycle listeners");
            _gameCycle.RegisterListeners();

            await tracker.Next("Core: Loading preferences");
            await _gameCycle.ExecuteConcurrent(this, ActionID.PREFERENCES_LOAD);

            await tracker.Next("Core: Warmup");
            await _gameCycle.ExecuteConcurrent(this, ActionID.WARMUP);

            BScene.CreateGameObject(this)
                .AddComponent<GameCycleMono>()
                .Initialize(_gameCycle);

            if (Application.isEditor)
            {
                await tracker.Next("Loading integrated save");
                
                await _save.LoadAsync(_integratedSave.model);
            
                await mainHUD.Open(default);
            
                return;
            }

            Action nextAction;

            bool saveExists = _save.Exists(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);
                
            if (saveExists)
            {
                await tracker.Next("Loading game save");
                ActionResult result = await _save.LoadAsync(SaveManager.DEFAULT_FOLDER, SaveManager.DEFAULT_FILE);

                if (result.IsSuccess())
                {
                    nextAction = () => mainHUD.Open(default);
                }
                else
                {
                    nextAction = () => mapGeneration.Open(default);
                }
            }
            else
            {
                nextAction = () => mapGeneration.Open(default);
            }

            await tracker.Next("Waiting for player input");

            await loadingScreen.Open(new LoadingScreenInput
            {
                clearLastProgress = false,
                currentProgress = 1,
                progressTracker = null,
                continueButtonAction = nextAction
            });
        }
    }
}