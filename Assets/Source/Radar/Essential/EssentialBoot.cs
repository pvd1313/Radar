using System.Collections.Generic;
using System.Threading;
using Beast.Claw.Attributes;
using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Resources;
using Beast.Radar.Core;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Beast.Radar.Essential
{
    public class EssentialBoot : Controller, IAsyncStartable
    {
        [Required] private readonly EssentialConfig _config;
        [Required] private readonly IWindowManager _windows;
        [Required] private readonly IGameLoop _gameLoop;
        [Required] private readonly IObjectResolver _scope;
        [Required] private readonly IResourceManager _resources;

        private ScriptableInstaller[] _coreInstallers;

        public async UniTask StartAsync(CancellationToken cancellation = new())
        {
            _gameLoop.Initialize(new GameLoopSetup
            {
                actions = new GameLoopActionList().All
            });

            _scope.TryResolve(out IReadOnlyList<IGameLoopActor> actors);
            
            _gameLoop.RegisterActors(actors);

            await _gameLoop.ExecuteConcurrent(this, ActionID.WARMUP);

            await _startupCore();
        }

        private async UniTask _startupCore()
        {
            ProgressTracker tracker = new();

            await _windows.Open.LoadingScreen(new LoadingWindowInput
            {
                progressTracker = tracker,
                clearLastProgress = true,
                currentProgress = 0,
                nextAction = null
            });

            tracker.Begin(3, 0.3f);

            await tracker.Next("Loading installers");
            _coreInstallers = new ScriptableInstaller[_config.coreInstallers.Count];
            for (int i = 0; i < _coreInstallers.Length; i++)
            {
                _coreInstallers[i] = await _resources.LoadAsset(this, _config.coreInstallers[i]);
            }

            await tracker.Next("Binding installers");
            
            IScopedObjectResolver resolver = _scope.CreateScope(container =>
            {
                foreach (ScriptableInstaller installer in _coreInstallers)
                {
                    installer.Configure(container);
                }
            });
            
            await tracker.Next("Booting core");
            await resolver.Resolve<ICoreBoot>().Boot(tracker);
        }
    }
}