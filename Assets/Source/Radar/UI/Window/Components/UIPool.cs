using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIPool<TPoolable> where TPoolable : UIView, IUIPoolable
    {
        private readonly IUIFactory _uiFactory;
        private readonly Transform _root;
        private readonly string _prefabID;
        private readonly List<TPoolable> _all = new();
        private readonly List<TPoolable> _enabled = new();

        public UIPool(IUIFactory uiFactory, string prefabID, Transform root)
        {
            _root = root;
            _uiFactory = uiFactory;
            _prefabID = prefabID;
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

        private TPoolable _getDisabledEntity()
        {
            return _all.FirstOrDefault(entity => entity != null && !entity.gameObject.activeSelf);
        }

        private TPoolable _addNewEntity()
        {
            _uiFactory.Create(this, _prefabID, out TPoolable entity);
            entity.transform.SetParent(_root, false);
            _all.Add(entity);
            return entity;
        }

        public TPoolable GetByIndex(int index)
        {
            return _enabled[index];
        }
    }
}