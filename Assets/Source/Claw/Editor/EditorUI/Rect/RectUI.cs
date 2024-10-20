using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beast.Claw.Editor
{
    public partial class RectUI
    {
        private readonly Dictionary<Type, Type> _taskToDrawerType = new();
        private readonly Dictionary<Type, object> _taskToObject = new();
        private readonly Dictionary<Type, Action> _taskToDrawAction = new();

        private object _currentTask;
        private Rect _currentRect;

        public void Bind<TDrawer, TDrawTask>()
            where TDrawer : class, IRectDrawer<TDrawTask>, new()
            where TDrawTask : class, IValidatable, new()
        {
            Type drawerType = typeof(TDrawer);
            Type taskType = typeof(TDrawTask);

            if (_taskToDrawerType.TryGetValue(taskType, out Type existingDrawer))
            {
                string message = $"Task '{taskType.FullName}' already binded to '{existingDrawer.FullName}'";
                throw new InvalidOperationException(message);
            }

            _taskToDrawerType[taskType] = drawerType;
            _taskToObject[taskType] = new TDrawTask();
            
            TDrawer drawer = new();
            drawer.Initialize(this);

            _taskToDrawAction[taskType] = () =>
            {
                drawer.Draw(_currentRect, (TDrawTask)_currentTask);
            };
        }

        public void Temp<TDrawTask>(out TDrawTask task) where TDrawTask : class
        {
            Type taskType = typeof(TDrawTask);

            if (!_taskToObject.TryGetValue(taskType, out object taskObject))
            {
                string message = $"No drawers binded for task '{taskType.FullName}'";
                throw new InvalidOperationException(message);
            }

            task = (TDrawTask)taskObject;
        }

        public void Draw<TDrawTask>(Rect rect, TDrawTask task) where TDrawTask : class, IValidatable
        {
            Type taskType = typeof(TDrawTask);

            if (!_taskToDrawAction.TryGetValue(taskType, out Action drawAction))
            {
                string message = $"No drawers binded for task '{taskType.FullName}'";
                throw new InvalidOperationException(message);
            }
            
            task.ValidateThrowException();

            _currentRect = rect;
            _currentTask = task;
            
            try
            {
                drawAction.Invoke();
            }
            finally
            {
                _currentTask = default;
                _currentTask = null;
            }
        }
    }
}