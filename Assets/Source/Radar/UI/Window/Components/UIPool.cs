using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Beast.Radar.UI
{
    public class UIPool<T> where T : UIView, IUIPoolEntity
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
            T entity = GetDisabledEntity() ?? AddNewEntity(initParams);
            _enabled.Add(entity);
            entity.Enable();
            return entity;
        }

        public IEnumerable<T> GetAll()
        {
            return _all;
        }

        public IEnumerable<T> GetAllEnabled()
        {
            return _enabled;
        }

        public void Disable(T entity)
        {
            if (entity == null || !entity.gameObject.activeSelf) return;
            entity.Disable();
            _enabled.Remove(entity);
        }

        public void DisableAll()
        {
            foreach (T entity in _enabled)
            {
                entity.Disable();
            }

            _enabled.Clear();
        }
        
        private T GetDisabledEntity()
        {
            return _all.FirstOrDefault(entity => entity != null && !entity.gameObject.activeSelf);
        }

        private T AddNewEntity(params object[] initParams)
        {
            _uiFactory.Create(this, _prefabID, out T entity);
            entity.transform.SetParent(_root, false);
            entity.Init(initParams);
            _all.Add(entity);
            return entity;
        }
    }
}