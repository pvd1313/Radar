using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIPool<T> where T : UIView, IUIPoolable
    {
        private readonly IUIFactory _uiFactory;
        private readonly Transform _root;
        private readonly string _prefabID;
        private readonly List<T> _all = new();
        private readonly List<T> _enabled = new();

        public UIPool(IUIFactory uiFactory, string prefabID, Transform root)
        {
            _root = root;
            _uiFactory = uiFactory;
            _prefabID = prefabID;
        }

        public T GetNext(params object[] initParams)
        {
            T entity = _getDisabledEntity() ?? _addNewEntity(initParams);
            _enabled.Add(entity);
            entity.Enable();
            return entity;
        }

        public IEnumerable<T> GetAllEnabled()
        {
            return _enabled;
        }

        public void DisableAll()
        {
            foreach (T entity in _enabled)
            {
                entity.Disable();
            }
            _enabled.Clear();
        }
        
        private T _getDisabledEntity()
        {
            return _all.FirstOrDefault(entity => entity != null && !entity.gameObject.activeSelf);
        }

        private T _addNewEntity(params object[] initParams)
        {
            _uiFactory.Create(this, _prefabID, out T entity);
            entity.transform.SetParent(_root, false);
            _all.Add(entity);
            return entity;
        }
    }
}