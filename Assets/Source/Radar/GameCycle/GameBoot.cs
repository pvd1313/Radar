using Beast.Claw.GameCycle;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar.GameCycle
{
    public class GameBoot : IGameCycleListener
    {
        private readonly IWindowManager _windows;
        private readonly IGameSaveManager _gameSave;

        public GameBoot(IWindowManager windows, IGameSaveManager gameSave)
        {
            _windows = windows;
            _gameSave = gameSave;
        }
        
        public void ListenGameCycle(IGameCycleHandle handle)
        {
            handle.ListenOperation(this, OperationID.Boot, Boot);
        }

        private UniTask<OperationResult> Boot()
        {
            if (Application.isEditor)
            {
                _gameSave.LoadDefaultSave();
            }
            else
            {
                _windows.Show(WindowID.MapGeneration);
            }

            return OperationResult.SuccessSync();
        }
    }
}