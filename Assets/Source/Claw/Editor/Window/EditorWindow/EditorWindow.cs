using System;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    [FancyAsset]
    public abstract class EditorWindow : UnityEditor.EditorWindow
    {
        public IFlow flow => _flow.Begin();

        private Flow _flow;
        private bool _initialized;
        private bool _criticalFault;
        
        protected EditorWindow()
        {
            AssemblyReloadEvents.beforeAssemblyReload += _beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += _afterAssemblyReload;
        }

        /// <summary>
        /// Used to reset window state for debug purposes. Invoked manually by user.
        /// Useful during window logic implementation.
        /// </summary>
        protected abstract void Reset();

        /// <summary>
        /// Called once before first draw call. Should be used for non-serializable structures creation.
        /// Should be used for GUI style / layout classes creation.
        /// And once before first draw call after each domain reload.
        /// </summary>
        protected abstract void Warmup();

        protected abstract void InspectorUI();
        
        /// <summary>
        /// Called when window closing. Called before assembly reload.
        /// </summary>
        protected abstract void Dispose();

        // Reset leads to: Dispose() -> Reset() -> Warmup()
        protected void DrawResetButton()
        {
            _flow.Begin().LabelWidth(UIStyle.SIZE_ONE_CHAR).Button("~", () =>
            {
                _safeDispose();
                
                _safeReset();

                throw new ExitGUIException();
            });
        }

        private void _beforeAssemblyReload()
        {
            _initialized = false;
            _criticalFault = false;
            
            Dispose();
        }

        private void _afterAssemblyReload()
        {
            AssemblyReloadEvents.beforeAssemblyReload += _beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += _afterAssemblyReload;
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
                InspectorUI();
                
                _flow.Close();
            }
            catch (ExitGUIException)
            {
                _flow.ExitGUIException();

                throw;
            }
        }

        private void _safeReset()
        {
            try
            {
                Reset();
            }
            catch (Exception e)
            {
                BDebug.LogException(this, e, GetType().FullName + ".SerializationReset()");
            }
        }

        private void _safeWarmup()
        {
            try
            {
                Warmup();

                _flow = new Flow();

                _initialized = true;
            }
            catch (Exception e)
            {
                _criticalFault = true;
                
                BDebug.LogException(this, e, GetType().FullName + ".Warmup(). Drawing is disabled. " +
                                             "Reload domain to continue.");
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
        }
    }
}