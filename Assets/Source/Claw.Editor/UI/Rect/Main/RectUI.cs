using System;
using System.Collections.Generic;
using Beast.Claw.Collections;
using Beast.Claw.Common;
using Beast.Claw.Editor.UI.UI;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Editor.UI
{
    public partial class RectUI
    {
        private const int POOL_CAPACITY = 16;
        
        private readonly Dictionary<Type, Type> _rectTaskToDrawer = new();
        private readonly Dictionary<Type, object> _rectTaskToPool = new();
        private readonly Dictionary<Type, Action> _rectTaskToAction = new();

        private readonly Dictionary<Type, Type> _fieldValueToDrawer = new();
        private readonly Dictionary<Type, object> _fieldValueToContainer = new();
        private readonly Dictionary<Type, Action> _fieldValueToAction = new();

        private readonly UIStyleGroup _style = UIStyleGroup.Default();

        private RingPool<UIAction> _indentPool = new(POOL_CAPACITY, () => new UIAction());
        
        private object _currentDrawTask;
        private Rect _currentScreen;
        private Label _currentLabel;
        private object _currentValueContainer;
        private int _tempContentIndex;

        public void BindDrawer<TDrawer, TDrawTask>()
            where TDrawer : class, IRectDrawer<TDrawTask>, new()
            where TDrawTask : class, IRectDrawTask, new()
        {
            Type drawerType = typeof(TDrawer);
            Type taskType = typeof(TDrawTask);

            if (_rectTaskToDrawer.TryGetValue(taskType, out Type existingDrawer))
            {
                string message = $"Task '{taskType.FullName}' is already binded to '{existingDrawer.FullName}'";
                throw new InvalidOperationException(message);
            }
            
            _rectTaskToDrawer[taskType] = drawerType;
            _rectTaskToPool[taskType] = new RingPool<TDrawTask>(POOL_CAPACITY, () => new TDrawTask());
            
            TDrawer drawer = new();
            drawer.Warmup(this, _style);

            _rectTaskToAction[taskType] = () =>
            {
                drawer.Draw(_currentScreen, (TDrawTask)_currentDrawTask);
            };
        }

        public void BindField<TDrawer, TValue>()
            where TDrawer : class, IFieldDrawer<TValue>, new()
        {
            Type drawerType = typeof(TDrawer);
            Type valueType = typeof(TValue);

            if (_fieldValueToDrawer.TryGetValue(valueType, out Type existingDrawer))
            {
                string message = $"Value '{valueType.FullName}' is already binded to '{existingDrawer.FullName}'";
                throw new InvalidOperationException(message);
            }

            _fieldValueToDrawer[valueType] = drawerType;
            _fieldValueToContainer[valueType] = new DataContainer<TValue>();

            TDrawer drawer = new();
            drawer.Warmup(this, _style);

            _fieldValueToAction[valueType] = () =>
            {
                FieldTask<TValue> task;

                DataContainer<TValue> container = (DataContainer<TValue>)_currentValueContainer;
                
                task.value = container.data;
                task.label = _currentLabel;
                task.screen = _currentScreen;

                container.data = drawer.Draw(task);
            };
        }

        public void TempTaskWithDefaultStyle<TDrawTask>(out TDrawTask task) where TDrawTask : class, IRectDrawTask, new()
        {
            Type taskType = typeof(TDrawTask);

            if (!_rectTaskToPool.TryGetValue(taskType, out object pool))
            {
                string message = $"No drawers binded for task '{taskType.FullName}'";
                throw new InvalidOperationException(message);
            }

            RingPool<TDrawTask> generic = (RingPool<TDrawTask>)pool;
            task = generic.Next();
            task.SetStyle(_style);
        }

        public TValue Field<TValue>(Rect rect, Label label, TValue value)
        {
            Type valueType = typeof(TValue);

            if (!_fieldValueToAction.TryGetValue(valueType, out Action drawAction))
            {
                string message = $"No field drawers binded for task '{valueType.FullName}'";
                throw new InvalidOperationException(message);
            }

            _currentScreen = rect;
            _currentLabel = label;
            _currentValueContainer = _fieldValueToContainer[valueType];

            try
            {
                DataContainer<TValue> valueContainer = _currentValueContainer as DataContainer<TValue>;

                valueContainer.data = value;
                
                drawAction.Invoke();

                return valueContainer.data;
            }
            finally
            {
                _currentLabel = default;
                _currentValueContainer = default;
            }
        }

        public TValue CustomField<TValue>(Rect rect, Label label, TValue value, IFieldDrawer<TValue> customDrawer)
        {
            return customDrawer.Draw(new FieldTask<TValue>
            {
                label = label,
                value = value,
                screen = rect
            });
        }

        public void Draw<TDrawTask>(Rect rect, TDrawTask task) where TDrawTask : class, IValidatable
        {
            Type taskType = typeof(TDrawTask);

            if (!_rectTaskToAction.TryGetValue(taskType, out Action drawAction))
            {
                string message = $"No rect drawers binded for task '{taskType.FullName}'";
                throw new InvalidOperationException(message);
            }
            
            task.ValidateThrowException(nameof(task));

            _currentScreen = rect;
            _currentDrawTask = task;
            
            try
            {
                drawAction.Invoke();
            }
            finally
            {
                _currentDrawTask = default;
                _currentDrawTask = null;
            }
        }
    }
}