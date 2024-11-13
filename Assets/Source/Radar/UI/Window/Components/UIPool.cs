using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Common;
using Unity.Mathematics;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIPool<TPoolable> where TPoolable : UIView, IUIPoolable
    {
        private readonly Transform _root;
        private readonly TPoolable _prefab;
        private readonly List<TPoolable> _all = new();
        private readonly List<TPoolable> _enabled = new();

        public UIPool(TPoolable prefab, Transform root)
        {
            _root = root;
            _prefab = prefab;
        }

        public TPoolable GetNext()
        {
            TPoolable entity = _getDisabledEntity() ?? _addNewEntity();
            _enabled.Add(entity);
            entity.Enable();
            return entity;
        }

        public IEnumerable<TPoolable> GetAllEnabled()
        {
            return _enabled;
        }

        public int GetEnabledCount()
        {
            return _enabled.Count;
        }

        public void DisableAll()
        {
            foreach (TPoolable entity in _enabled)
            {
                entity.Disable();
            }
            _enabled.Clear();
        }

        public void Disable(TPoolable entity)
        {
            _enabled.Remove(entity);
            entity.Disable();
        }

        public void Enable(TPoolable entity)
        {
            _enabled.Add(entity);
            entity.Enable();
        }

        public TPoolable GetEnabledByIndex(int index)
        {
            return _enabled[index];
        }

        public TPoolable GetByIndex(int index)
        {
            if (index >= _all.Count)
            {
                return null;
            }

            return _all[index];
        }

        private TPoolable _getDisabledEntity()
        {
            return _all.FirstOrDefault(entity => entity != null && !entity.gameObject.activeSelf);
        }

        private TPoolable _addNewEntity()
        {
            TPoolable entity = BScene.InstantiateMono(this, _prefab, float3.zero, quaternion.identity);
            entity.transform.SetParent(_root, false);
            _all.Add(entity);
            return entity;
        }
    }
}