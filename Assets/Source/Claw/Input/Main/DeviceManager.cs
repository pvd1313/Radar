using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameLoop;
using Beast.Claw.Graphic;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.Input
{
    public class DeviceManager : Actor, IDeviceManager
    {
        public IInputDevice ActiveDevice => _activeDevice;

        private readonly InputConfig _config;
        private readonly IInputManager _inputManager;
        private readonly IRuntime _runtime;
        private readonly IInputDriver _driver;

        private IInputDevice _mouse;
        private IInputDevice _touchScreen;
        private IInputDevice _activeDevice;

        public DeviceManager(
            InputConfig config, 
            IInputManager inputManager, 
            IRuntime runtime,
            IInputDriver driver)
        {
            _config = config;
            _inputManager = inputManager;
            _runtime = runtime;
            _driver = driver;
        }

        protected override async UniTask _warmup(ConcurrentContext context)
        {
            // Now we are simply binding based on platform type. But in future we would bind multiple device to control
            // separate screen parts. Like twin stick shooter has two dpad touch controls for movement & shooting.
            // Also, maybe create factory for devices. For now, we can not bind device into container, as to call _warmup
            // we need to call GameCycle.Execute... Maybe we could create separate event for device warmup... But
            // creating event for devices sounds like overhead. Let's now just instantiate them. Actually manual control
            // gives us advantages, because we can easily swap devices.
            
            _mouse = new Mouse(_driver, _inputManager, _config);
            _touchScreen = new TouchScreen(_driver, _inputManager, _config);

            await _mouse.Warmup(context);
            await _touchScreen.Warmup(context);

            _deviceSwitchLogic();
        }

        protected override void _tick(BlockingContext context)
        {
            _deviceSwitchLogic();

            _activeDevice.Tick(context);
        }

        private void _deviceSwitchLogic()
        {
            IInputDevice desiredDevice;

            if (Build.IS_MOBILE_PLATFORM || _runtime.IsMobileSimulatorActive)
            {
                desiredDevice = _touchScreen;
            }
            else
            {
                desiredDevice = _mouse;
            }

            if (desiredDevice != _activeDevice)
            {
                if (_activeDevice != null)
                {
                    _activeDevice.Deactivate();
                    
                    // In a very distant future we can fire event about device switch.
                    // We could support multiple active devices as well.
                    BDebug.LogInfo(
                        this, 
                        $"Switched: ({_activeDevice.FriendlyName} -> {desiredDevice.FriendlyName})");
                }
                else
                {
                    BDebug.LogInfo(
                        this, 
                        $"Active: {desiredDevice.FriendlyName}");
                }
                
                _activeDevice = desiredDevice;
                
                _activeDevice.Activate();
            }
        }

        protected override void _dispose(BlockingContext context)
        {
            _mouse.Dispose(context);
            _touchScreen.Dispose(context);
        }
    }
}