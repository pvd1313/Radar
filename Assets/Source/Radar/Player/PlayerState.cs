using System.Collections.Generic;

namespace Beast.Radar.Player
{
    public class PlayerState : IPlayerState
    {
        private readonly Dictionary<string, object> _states = new();

        public void SetState<TValue>(string tag, TValue value)
        {
            _states[tag] = value;
        }

        public bool TryGetState<TValue>(string tag, out TValue value)
        {
            if (_states.TryGetValue(tag, out object objValue))
            {
                if (objValue is TValue typedValue) 
                {
                    value = typedValue;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}