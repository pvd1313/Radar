using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Cysharp.Threading.Tasks;

using Application = UnityEngine.Application;
using UnityInput = UnityEngine.Input;

namespace Beast.Claw.Input
{
    public class DeviceManager : Controller, IDeviceManager, IGameCycleListener
    {
        public IInputDevice ActiveDevice => _activeDevice;

        private readonly IScreen _screen;
        private readonly InputConfig _config;
        private readonly IInputManager _inputManager;
        private readonly IRuntime _runtime;

        private IInputDevice _mouse;
        private IInputDevice _touchScreen;

        private IInputDevice _activeDevice;

        public DeviceManager(
            IScreen screen, 
            InputConfig config, 
            IInputManager inputManager, 
            IRuntime runtime)
        {
            _screen = screen;
            _config = config;
            _inputManager = inputManager;
            _runtime = runtime;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        private async UniTask _warmup(ConcurrentContext context)
        {
            UnityInput.gyro.enabled = false;
            UnityInput.compass.enabled = false;
            UnityInput.compensateSensors = false;
            UnityInput.simulateMouseWithTouches = false;

            // Now we are simply binding based on platform type. But in future we would bind multiple device to control
            // separate screen parts. Like twin stick shooter has two dpad touch controls for movement & shooting.
            // Also, maybe create factory for devices. For now, we can not bind device into container, as to call _warmup
            // we need to call GameCycle.Execute... Maybe we could create separate event for device warmup... But
            // creating event for devices sounds like overhead. Let's now just instantiate them. Actually manual control
            // gives us advantages, because we can easily swap devices.
            
            _mouse = new Mouse(_screen, _inputManager, _config.mouse);
            _touchScreen = new TouchScreen(_screen, _inputManager, _config.touchScreen);

            await _mouse.Warmup(context);
            await _touchScreen.Warmup(context);

            _deviceSwitchLogic();
        }
        
        private void _tick(BlockingContext context)
        {
            _deviceSwitchLogic();

            _activeDevice.Tick(context);
        }

        private void _deviceSwitchLogic()
        {
            IInputDevice desiredDevice;

            if (Application.isMobilePlatform || _runtime.IsMobileSimulatorActive)
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
        
        private void _dispose(BlockingContext context)
        {
            _mouse.Dispose(context);
            _touchScreen.Dispose(context);
        }
    }
}