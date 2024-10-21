using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class RectUI
    {
        public UIStyleSet DefaultStyleSet { get; } = new();
        
        private readonly Dictionary<Type, Type> _rectTaskToDrawer = new();
        private readonly Dictionary<Type, object> _rectTaskToObject = new();
        private readonly Dictionary<Type, Action> _rectTaskToAction = new();

        private readonly Dictionary<Type, Type> _fieldValueToDrawer = new();
        private readonly Dictionary<Type, object> _fieldValueToContainer = new();
        private readonly Dictionary<Type, Action> _fieldValueToAction = new();

        private object _currentDrawTask;
        private Rect _currentScreen;
        private Label _currentLabel;
        private object _currentValueContainer;

        public void BindRect<TDrawer, TDrawTask>()
            where TDrawer : class, IRectDrawer<TDrawTask>, new()
            where TDrawTask : class, IValidatable, new()
        {
            Type drawerType = typeof(TDrawer);
            Type taskType = typeof(TDrawTask);

            if (_rectTaskToDrawer.TryGetValue(taskType, out Type existingDrawer))
            {
                string message = $"Task '{taskType.FullName}' is already binded to '{existingDrawer.FullName}'";
                throw new InvalidOperationException(message);
            }

            _rectTaskToDrawer[taskType] = drawerType;
            _rectTaskToObject[taskType] = new TDrawTask();
            
            TDrawer drawer = new();
            drawer.Initialize(this);

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
            _fieldValueToContainer[valueType] = new ValueContainer<TValue>();

            TDrawer drawer = new();
            drawer.Initialize(this);

            _fieldValueToAction[valueType] = () =>
            {
                FieldTask<TValue> task;
                task.data = (ValueContainer<TValue>)_currentValueContainer;
                task.label = _currentLabel;
                task.screen = _currentScreen;

                drawer.Draw(task);
            };
        }

        public void TempTask<TDrawTask>(out TDrawTask task) where TDrawTask : class
        {
            Type taskType = typeof(TDrawTask);

            if (!_rectTaskToObject.TryGetValue(taskType, out object taskObject))
            {
                string message = $"No drawers binded for task '{taskType.FullName}'";
                throw new InvalidOperationException(message);
            }

            task = (TDrawTask)taskObject;
        }

        public Label TempLabel(string text)
        {
            // TODO : #R2 This is broken;
            return EditorGUIUtility.TrTempContent(text);
        }

        public TValue DrawField<TValue>(Rect rect, Label label, TValue value)
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
                ValueContainer<TValue> valueContainer = _currentValueContainer as ValueContainer<TValue>;

                valueContainer.value = value;
                
                drawAction.Invoke();

                return valueContainer.value;
            }
            finally
            {
                _currentLabel = default;
                _currentValueContainer = default;
            }
        }

        public void DrawRect<TDrawTask>(Rect rect, TDrawTask task) where TDrawTask : class, IValidatable
        {
            Type taskType = typeof(TDrawTask);

            if (!_rectTaskToAction.TryGetValue(taskType, out Action drawAction))
            {
                string message = $"No rect drawers binded for task '{taskType.FullName}'";
                throw new InvalidOperationException(message);
            }
            
            task.ValidateThrowException();

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