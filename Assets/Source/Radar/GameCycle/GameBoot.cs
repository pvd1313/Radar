using Beast.Claw.GameCycle;
using Beast.Radar.Debug;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.GameCycle
{
    public class GameBoot : IGameBoot
    {
        private readonly IWindowManager _windows;
        private readonly IGameSaveManager _gameSave;

        public GameBoot(IWindowManager windows, IGameSaveManager gameSave)
        {
            _windows = windows;
            _gameSave = gameSave;
        }
        
        public UniTask<OperationResult> Boot()
        {
            if (Application.isEditor)
            {
                _gameSave.LoadDefaultSave();
                _windows.Show(WindowID.MainHUD);
            }
            else
            {
                _windows.Show(WindowID.MapGeneration);
            }

            return OperationResult.SuccessSync();
        }
    }
}