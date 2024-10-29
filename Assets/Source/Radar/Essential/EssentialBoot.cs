using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Resources;
using Beast.Radar.Core;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using Zenject;
using ResourceManager = System.Resources.ResourceManager;

namespace Beast.Radar.Essential
{
    public class EssentialBoot : Controller, IInitializable
    {
        private readonly EssentialConfig _config;
        private readonly IWindowManager _windows;
        private readonly IGameCycle _gameCycle;
        private readonly DiContainer _container;

        private ScriptableInstaller[] _coreInstallers;

        public EssentialBoot(
            EssentialConfig config, 
            IWindowManager windows, 
            IGameCycle gameCycle,
            DiContainer container)
        {
            _config = config;
            _windows = windows;
            _gameCycle = gameCycle;
            _container = container;
        }
        
        public async void Initialize()
        {
            _gameCycle.Initialize(new GameCycleInitialization
            {
                actions = new GameCycleActionList().All
            });
            
            _gameCycle.RegisterListeners();

            await _gameCycle.Execute(this, ActionID.WARMUP);

            await _windows.GetHandle<Empty>(this, WindowID.LoadingScreen).Open(default);

            await _startupCore();
        }

        private async UniTask _startupCore()
        {
            _coreInstallers = new ScriptableInstaller[_config.installers.Count];
            for (int i = 0; i < _coreInstallers.Length; i++)
            {
                _coreInstallers[i] = await BResource.LoadAsset(this, _config.installers[i]);
            }

            foreach (ScriptableInstaller installer in _coreInstallers)
            {
                _container.Inject(installer);
                
                installer.InstallBindings();
            }

            await _container.Resolve<ICoreBoot>().Boot();
        }
    }
}