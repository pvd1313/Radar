using Beast.Claw.GameCycle;
using Beast.Radar.GameCycle;
using UnityEngine;

namespace Beast.Radar.Debug
{
    public class DebugPlayerPreferencesExamplePlayerTimeRecorderThatShouldBeEliminatedAsSoonAsPossible : IGameCycleListener
    {
        private readonly DebugPreferences _preferences;

        public DebugPlayerPreferencesExamplePlayerTimeRecorderThatShouldBeEliminatedAsSoonAsPossible(DebugPreferences preferences)
        {
            _preferences = preferences;
        }
        
        public void Register(IGameCycleHandle handle)
        {
            handle.ListenUpdate(this, UpdateID.Tick, _tick);
        }

        private void _tick()
        {
            _preferences
                    .playerPlayTimeThanShouldBeDefinitelyStoredNotHereAndBeWithAMuchShorterNameAsShortNamesAreMuchEasierToReadAndRemember
                = Time.realtimeSinceStartup;
        }
    }
}