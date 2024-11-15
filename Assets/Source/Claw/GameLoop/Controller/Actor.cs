using Beast.Claw.Common;
using Cysharp.Threading.Tasks;

namespace Beast.Claw.GameLoop
{
    public class Actor : Controller, IGameLoopActor
    {
        public void Register(IGameLoopHandle handle)
        {
            handle.ListenConcurrent(ActionID.WARMUP, _warmup);
            handle.ListenConcurrent(ActionID.PREFERENCES_LOAD, _preferencesLoad);
            handle.ListenConcurrent(ActionID.GAME_LOAD, _gameLoad);
            
            handle.ListenBlocking(ActionID.TICK, _tick);
            handle.ListenBlocking(ActionID.RENDER, _render);
            handle.ListenBlocking(ActionID.FIXED_UPDATE, _fixed);
            
            handle.ListenBlocking(ActionID.GAME_SAVE, _gameSave);
            handle.ListenBlocking(ActionID.PREFERENCES_SAVE, _preferencesSave);
            handle.ListenBlocking(ActionID.DISPOSE, _dispose);
        }

        // Rare concurrent.
        protected virtual UniTask _warmup(ConcurrentContext context)
        {
            // Called once. No need to discard.
            return UniTask.CompletedTask;
        }
        
        protected virtual UniTask _preferencesLoad(ConcurrentContext context)
        {
            // Called once. No need to discard. Probably redundant method.
            return UniTask.CompletedTask;
        }
        
        protected virtual UniTask _gameLoad(ConcurrentContext context)
        {
            return context.CancelEmptyMethod();
        }
        
        // Regular blocking.
        protected virtual void _tick(BlockingContext context)
        {
            context.CancelEmptyMethod();
        }
        
        protected virtual void _fixed(BlockingContext context)
        {
            context.CancelEmptyMethod();
        }
        
        protected virtual void _render(BlockingContext context)
        {
            context.CancelEmptyMethod();
        }
        
        // Rare blocking.
        protected virtual void _gameSave(BlockingContext context)
        {
            context.CancelEmptyMethod();
        }
        
        protected virtual void _preferencesSave(BlockingContext context)
        {
            context.CancelEmptyMethod(); 
        }
        
        protected virtual void _dispose(BlockingContext obj)
        {
            // Called once. No need to discard.
        }
    }
}