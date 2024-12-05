using Beast.Claw.Common;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Beast.Claw.Graphy.Modules;
using Beast.Claw.Graphy.Modules.Ram;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Claw.Graphy
{
    public class RadarGraphyManager : Actor, IGraphyManager, IReady 
    {
        public FpsMonitor FpsMonitor { get; private set; }
        public FpsRender FpsRender { get; private set;}
        public InformationMonitor InformationMonitor { get; private set; }
        public AudioMonitor AudioMonitor { get; private set; }
        public AudioRender AudioRender { get; private set; }
        public RamMonitor RamMonitor { get; private set; }
        public RamRender RamRender { get; private set; }
        
        public bool IsReady { get; private set; }
        
        private readonly GraphyConfig _config;
        private readonly ICamera _camera;
        
        protected RadarGraphyManager(GraphyConfig config, ICamera camera)
        {
            _config = config;
            _camera = camera;
        }
        
        protected override UniTask _warmup(ConcurrentContext context)
        {
            FpsMonitor = new FpsMonitor();
            FpsMonitor.Initialize();
            FpsRender = new FpsRender(_config, FpsMonitor);
            FpsRender.Initialize();

            InformationMonitor = new InformationMonitor();
            InformationMonitor.Initialize();

            AudioMonitor = new AudioMonitor(_config, _camera.Camera.GetComponent<AudioListener>());
            AudioMonitor.Initialize();
            AudioRender = new AudioRender(_config, AudioMonitor);
            AudioRender.Initialize();

            RamMonitor = new RamMonitor();
            RamMonitor.Initialize();
            RamRender = new RamRender(_config, RamMonitor);
            RamRender.Initialize();
            
            IsReady = true;

            return UniTask.CompletedTask;
        }

        protected override void _tick(BlockingContext context)
        {
            if (!IsReady)
            {
                return;
            }
            
            FpsMonitor.Update();
            FpsRender.Update();
            InformationMonitor.Update();
            AudioMonitor.Update();
            AudioRender.Update();
            RamMonitor.Update();
            RamRender.Update();
        }
        
        protected override void _dispose(BlockingContext obj)
        {
            FpsRender.Dispose();
            AudioRender.Dispose();
            RamRender.Dispose();
        }
    }
}
