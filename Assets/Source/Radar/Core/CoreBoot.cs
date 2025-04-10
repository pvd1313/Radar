using System;
using System.Collections.Generic;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.ProgressTracker;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Beast.Radar.Core
{
    public class CoreBoot : Controller, ICoreBoot
    {
        [Required] private readonly IWindowManager _window;
        [Required] private readonly ISaveManager _saveManager;
        [Required] private readonly IntegratedSaveConfig _integratedSave;
        [Required] private readonly IGameLoop _gameLoop;
        [Required] private readonly IWorldManager _worldManager;
        [Required] private readonly IObjectResolver _container;

        public async UniTask Boot(IProgressTracker tracker)
        {
            tracker.Begin(4, 1 - tracker.Progress);

            await tracker.Next("Core: Registering GameLoop actors");
            _container.TryResolve(out IReadOnlyList<IGameLoopActor> actors);
            _gameLoop.RegisterActors(actors);

            await tracker.Next("Core: Loading preferences");
            await _gameLoop.ExecuteConcurrent(this, ActionID.PREFERENCES_LOAD, null);

            await tracker.Next("Core: Warmup");
            await _gameLoop.ExecuteConcurrent(this, ActionID.WARMUP, null);

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
                
                await _saveManager.LoadAsync(_integratedSave.gameModel, ESaveManagerFlags.Silent);
            
                await _window.Open.HUD();
            
                return;
            }

            await _window.Open.MainMenu();
        }
    }
}