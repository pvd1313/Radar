using System.Collections.Generic;
using Beast.Claw.GameCycle;
using Beast.Radar.UI;

namespace Beast.Radar.Essential
{
    public class EssentialBoot : Zenject.IInitializable
    {
        private readonly EssentialConfig _config;
        private readonly IWindowManager _windows;
        private readonly IGameCycle _gameCycle;

        public EssentialBoot(
            EssentialConfig config, 
            IWindowManager windows, 
            IGameCycle gameCycle)
        {
            _config = config;
            _windows = windows;
            _gameCycle = gameCycle;
        }
        
        public async void Initialize()
        {
            _gameCycle.Initialize(new GameCycleData
            {
                operationEvents = new List<OperationInfo>
                {
                    new (OperationID.WARMUP, true),
                    new (OperationID.DISPOSE, true),
                    
                    new (OperationID.GAME_SAVE, false),
                    new (OperationID.GAME_LOAD, false),
                    
                    new (OperationID.PREFERENCES_SAVE, false),
                    new (OperationID.PREFERENCES_LOAD, false)
                },
                
                updateEvents = new List<string>
                {
                    UpdateID.TICK,
                    UpdateID.RENDER,
                    UpdateID.FIXED_UPDATE,
                    UpdateID.GIZMOS_DRAW
                }
            });
            
            _gameCycle.RegisterListeners();

            await _gameCycle.Execute(this, OperationID.WARMUP);

            _windows.Show(WindowID.LoadingScreen);
        }
    }
}