using System.Collections;
using RisenBeast.Common;
using RisenBeast.Coroutines;
using RisenBeast.Debugging;
using RisenBeast.GameCycle;
using Zenject;

namespace RisenBeast.Core
{
    public class CoreController : IInitializable
    {
        private readonly DiContainer _container;
        private readonly CoroutineController _coroutineController;

        public CoreController (DiContainer container, CoroutineController coroutineController)
        {
            _container = container;
            _coroutineController = coroutineController;
        }
        
        public void Initialize()
        {
            _coroutineController.Initialize();

            _coroutineController.StartCoroutine(InitializationRoutine());
        }

        private IEnumerator InitializationRoutine()
        {
            yield return _container.TryResolve<GizmosController>()?.Initialize();
            
            UpdatingBehaviour behaviour = SceneUtil.NewObjectFor<CoreController>().AddComponent<UpdatingBehaviour>();
            behaviour.Updated += OnUpdate;
            behaviour.FixedUpdated += OnFixedUpdate;
            behaviour.LateUpdated += OnLateUpdate;

            yield return null;
        }

        private void OnFixedUpdate()
        {
            // _container.TryResolve<PlayerController>()?.FixedUpdate();
        }

        private void OnUpdate()
        {
            // _container.TryResolve<PlayerInputController>()?.Update();
        }
        
        private void OnLateUpdate()
        {
            
        }
    }
}