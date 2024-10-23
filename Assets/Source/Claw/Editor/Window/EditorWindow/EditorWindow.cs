using System;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    [FancyAsset]
    public abstract class EditorWindow : UnityEditor.EditorWindow, IHasCustomMenu
    {
        protected IFlow Begin => _flow.Begin();

        private Flow _flow;
        protected RectUI _rectUI;
        
        private bool _initialized;
        private bool _criticalFault;
        
        protected EditorWindow()
        {
            _subscribeUnityEvents();
        }
        
        public void AddItemsToMenu(GenericMenu menu)
        {
            Label message = "Reset";
            
            menu.AddItem(message.content, false, _ =>
            {
                _safeDispose();
                
                _safeReset();
                
                Repaint();
                
            }, null);
        }
        
        /// <summary>
        /// Called once before first draw call. Should be used for non-serializable structures creation.
        /// Should be used for GUI style / layout classes creation.
        /// And once before first draw call after each domain reload.
        /// </summary>
        protected abstract void Warmup();

        /// <summary>
        /// Used to reset window state for debug purposes. Invoked manually by user.
        /// Useful during window logic implementation. Dispose() -> Reset() -> Warmup()
        /// </summary>
        protected abstract void Reset();

        /// <summary>
        /// Use flow { get; } to begin drawing. flow { get; } returns _flow.Begin()
        /// </summary>
        protected abstract void DrawUI();
        
        /// <summary>
        /// Called when window closing. Called before assembly reload.
        /// </summary>
        protected abstract void Dispose();

        private void _beforeAssemblyReload()
        {
            _initialized = false;
            _criticalFault = false;
            
            Dispose();
        }

        private void _afterAssemblyReload()
        {
            _subscribeUnityEvents();
        }

        private void OnGUI()
        {
            if (_criticalFault)
            {
                return;
            }

            if (!_initialized)
            {
                _safeWarmup();

                if (!_initialized)
                {
                    return;
                }
            }

            try
            {
                DrawUI();
                
                _flow.Close();
            }
            catch (ExitGUIException)
            {
                _flow.ExitGUIException();

                throw;
            }
        }
        
        private void _subscribeUnityEvents()
        {
            AssemblyReloadEvents.beforeAssemblyReload += _beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += _afterAssemblyReload;
            Undo.postprocessModifications += _postProcessModifications;
            Undo.undoRedoEvent += _undoRedoEvent;
        }

        private void _unsubscribeUnityEvents()
        {
            AssemblyReloadEvents.beforeAssemblyReload -= _beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= _afterAssemblyReload;
            Undo.postprocessModifications -= _postProcessModifications;
            Undo.undoRedoEvent -= _undoRedoEvent;
        }

        private void _undoRedoEvent(in UndoRedoInfo undo)
        {
            Repaint();
        }

        private UndoPropertyModification[] _postProcessModifications(UndoPropertyModification[] modifications)
        {
            Repaint();

            return modifications;
        }

        private void _safeReset()
        {
            try
            {
                Reset();
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, this.TypeName() + ".SerializationReset()");
            }
        }

        private void _safeWarmup()
        {
            try
            {
                _rectUI = new RectUI();
                _flow = new Flow(_rectUI);
                new CommonDrawersContainer().BindAllTo(_rectUI);
                
                Warmup();

                _initialized = true;
            }
            catch (Exception e)
            {
                _criticalFault = true;

                string message = $"{this.TypeName()}.Warmup(). Drawing is disabled. Reload domain to continue.";
                
                BDebug.LogException(this, e, message);
            }
        }

        private void _safeDispose()
        {
            try
            {
                Dispose();

                _initialized = false;
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, GetType().FullName + ".Dispose()");
            }
        }

        private void OnDestroy()
        {
            _safeDispose();

            _unsubscribeUnityEvents();
        }
    }
}