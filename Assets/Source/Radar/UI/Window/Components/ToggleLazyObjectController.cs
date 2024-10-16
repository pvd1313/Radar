using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class ToggleLazyObjectController
    {
        private readonly UIToggle _toggle;
        private readonly GameObject _prefab;
        private GameObject _objectInstance;

        public ToggleLazyObjectController(UIToggle toggle, GameObject prefab)
        {
            _toggle = toggle;
            _prefab = prefab;
        }

        public void Warmup()
        {
            _toggle.Changed += _onToggleChanged;
        }

        public void Dispose()
        {
            _toggle.Changed -= _onToggleChanged;
        }

        private void _onToggleChanged(bool isOn)
        {
            _objectInstance ??= BScene.InstantiateGameObject(this, _prefab, float3.zero, quaternion.identity);
            _objectInstance.SetActive(isOn);
        }
    }
}