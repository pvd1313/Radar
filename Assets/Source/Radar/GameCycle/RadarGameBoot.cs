using Beast.Claw.GameCycle;
using Beast.Radar.Serialization;
using Beast.Radar.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Beast.Radar
{
    public class RadarGameBoot : IBeastGameCycleListener
    {
        private readonly IBeastWindowManager _windows;
        private readonly IBeastGameSaveManager _gameSave;

        public RadarGameBoot(IBeastWindowManager windows, IBeastGameSaveManager gameSave)
        {
            _windows = windows;
            _gameSave = gameSave;
        }
        
        public void ListenGameCycle(IBeastGameCycleHandle handle)
        {
            handle.ListenOperation(this, BeastCycleEventID.Boot, OnGameBoot);
        }

        private UniTask<BeastInitializationResult> OnGameBoot()
        {
            if (Application.isEditor)
            {
                _gameSave.LoadDefaultSave();
            }
            else
            {
                _windows.Show(BeastUIWindowID.MapGeneration);
            }

            return BeastInitializationResult.SuccessSync();
        }
    }
}