using System;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.ProgressTracker;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;

namespace Beast.Radar.Core
{
    public class CoreBoot : Controller, ICoreBoot
    {
        private readonly IWindowManager _window;
        private readonly ISaveManager _save;
        private readonly IntegratedSave _integratedSave;
        private readonly IGameLoop _gameLoop;
        private readonly IWorldManager _worldManager;

        public CoreBoot(
            IWindowManager window, 
            ISaveManager save, 
            IntegratedSave integratedSave, 
            IGameLoop gameLoop, 
            IWorldManager worldManager)
        {
            _window = window;
            _save = save;
            _integratedSave = integratedSave;
            _gameLoop = gameLoop;
            _worldManager = worldManager;
        }
        
        public async UniTask Boot(IProgressTracker tracker)
        {
            IWindowHandle<Empty> mainHUD = _window.GetHandle<Empty>(this, WindowID.HUD);
            IWindowHandle<Empty> mapGeneration = _window.GetHandle<Empty>(this, WindowID.MAP_GENERATION);
            IWindowHandle<LoadingWindowInput> loadingScreen = _window.GetHandle<LoadingWindowInput>(this, WindowID.LOADING);

            tracker.Begin(4, 1 - tracker.Progress);

            await tracker.Next("Core: Registering GameCycle listeners");
            _gameLoop.RegisterListeners();

            await tracker.Next("Core: Loading preferences");
            await _gameLoop.ExecuteConcurrent(this, ActionID.PREFERENCES_LOAD);

            await tracker.Next("Core: Warmup");
            await _gameLoop.ExecuteConcurrent(this, ActionID.WARMUP);

            BScene.CreateGameObject(this)
                .AddComponent<GameLoopMono>()
                .Initialize(_gameLoop);

            if (Build.IS_EDITOR)
            {
                await tracker.Next("Loading integrated save");
            
                Guid worldGuid = Guid.Parse(_integratedSave.worldGuid);
                _integratedSave.worldData.guid = worldGuid;
                _integratedSave.gameModel.world = worldGuid;
                
                _worldManager.AddWorld(_integratedSave.worldData);
                
                await _save.LoadAsync(_integratedSave.gameModel);
            
                await mainHUD.Open(default);
            
                return;
            }

            Action nextAction;

            bool saveExists = _save.Exists(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);
                
            if (saveExists)
            {
                await tracker.Next("Loading game save");
                EActionStatus result = await _save.LoadAsync(FolderName.DEFAULT_SAVE, FileName.DEFAULT_SAVE);

                if (result == EActionStatus.Success)
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

            await loadingScreen.Open(new LoadingWindowInput
            {
                clearLastProgress = false,
                currentProgress = 1,
                progressTracker = null,
                nextAction = nextAction
            });
        }
    }
}