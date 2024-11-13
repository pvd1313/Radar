using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Resources;
using Beast.Radar.Core;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Beast.Radar.Essential
{
    public class EssentialBoot : Controller, IInitializable
    {
        private readonly EssentialConfig _config;
        private readonly IWindowManager _windows;
        private readonly IGameCycle _gameCycle;
        private readonly DiContainer _container;
        private readonly IResourceManager _resources;

        private ScriptableInstaller[] _coreInstallers;

        public EssentialBoot(
            EssentialConfig config, 
            IWindowManager windows, 
            IGameCycle gameCycle,
            DiContainer container,
            IResourceManager resources)
        {
            _config = config;
            _windows = windows;
            _gameCycle = gameCycle;
            _container = container;
            _resources = resources;
        }
        
        public async void Initialize()
        {
            _gameCycle.Initialize(new GameCycleInitialization
            {
                actions = new GameCycleActionList().All
            });
            
            _gameCycle.RegisterListeners();

            await _gameCycle.ExecuteConcurrent(this, ActionID.WARMUP);

            await _startupCore();
        }

        private async UniTask _startupCore()
        {
            ProgressTracker tracker = new ();

            await _windows.GetHandle<LoadingScreenInput>(this, WindowID.LoadingScreen)
                .Open(new LoadingScreenInput
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
            foreach (ScriptableInstaller installer in _coreInstallers)
            {
                _container.Inject(installer);
                
                installer.InstallBindings();
            }
            
            if (Build.IS_DEVELOPMENT)
            {
                ScriptableInstaller debugInstaller = await _resources.LoadAsset(this, _config.debugInstaller);
                
                _container.Inject(debugInstaller);
                
                debugInstaller.InstallBindings();
            }
            
            await tracker.Next("Booting core");
            await _container.Resolve<ICoreBoot>().Boot(tracker);
        }
    }
}