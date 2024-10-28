using Beast.Claw.Common;
using Beast.Claw.GameCycle;
using Beast.Claw.Graphic;
using Beast.Radar.InputHandling;
using Beast.Radar.Serialization;
using Beast.Radar.World;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public class PlayerController : IGameCycleListener
    {
        private readonly ICameraTransform _camera;
        private readonly PrefabLibrary _library;
        private readonly PlayerConfig _config;
        private readonly IInputSystem _input;
        private readonly IWorldTime _time;
        private readonly GameModel _game;
        
        private IPlayerPosition _position;
        
        private PlayerView _view;
        
        public PlayerController(
            ICameraTransform camera,
            PrefabLibrary library,
            PlayerConfig config,
            IInputSystem input,
            GameModel game,
            IWorldTime time)
        {
            _camera = camera;
            _library = library;
            _config = config;
            _input = input;
            _game = game;
            _time = time;
        }

        public void Register(IGameCycleHandle handle)
        {
            handle.ListenOperation(ActionID.WARMUP, _warmup);
            handle.ListenOperation(ActionID.GAME_LOAD, _gameLoad);
            handle.ListenUpdate(ActionID.TICK, _tick);
            handle.ListenOperation(ActionID.GAME_SAVE, _gameSave);
            handle.ListenOperation(ActionID.DISPOSE, _dispose);
        }

        private UniTask<ActionResult> _warmup(ActionContext context)
        {
            _view = BScene.InstantiateMono(this, _library.playerViewPrefab, float3.zero, quaternion.identity);
            
            _position = new PlayerPosition(_view, _time, _input, _config, _camera);
            
            return ActionResult.SuccessSyncronous();
        }

        private void _tick(ActionContext context)
        {
            _position.Update();
        }

        private UniTask<ActionResult> _gameLoad(ActionContext arg)
        {
            _position.SetPosition(_game.player.position);
            
            return ActionResult.SuccessSyncronous();
        }

        private UniTask<ActionResult> _gameSave(ActionContext arg)
        {
            _game.player.position = _position.GetPosition();

            return ActionResult.SuccessSyncronous();
        }

        private UniTask<ActionResult> _dispose(ActionContext context)
        {
            return ActionResult.SuccessSyncronous();
        }
    }
}