using System;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using UnityEditor;
using UnityEngine;

namespace Beast.Claw.Editor.Window
{
    [FancyAsset]
    public abstract partial class EditorWindow : UnityEditor.EditorWindow
    {
        public IEditorFlow flow => _flow.Begin();

        private EditorFlow _flow;
        private bool _initialized;
        private bool _gotException;

        /// <summary>
        /// Called once before first draw call.
        /// And once before first draw call after each domain reload.
        /// </summary>
        protected abstract void Warmup();

        protected abstract void InspectorUI();
        
        /// <summary>
        /// Called when window closing. Called before assembly reload.
        /// </summary>
        protected abstract void Dispose();

        protected EditorWindow()
        {
            AssemblyReloadEvents.beforeAssemblyReload += _beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += _afterAssemblyReload;
        }

        private void _beforeAssemblyReload()
        {
            _initialized = false;
            _gotException = false;
            
            Dispose();
        }

        private void _afterAssemblyReload()
        {
            AssemblyReloadEvents.beforeAssemblyReload += _beforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += _afterAssemblyReload;
        }

        private void OnGUI()
        {
            if (_gotException)
            {
                return;
            }

            if (!_initialized)
            {
                try
                {
                    Warmup();
                    
                    _flow = new EditorFlow();
                    
                    _initialized = true;
                }
                catch (Exception e)
                {
                    string message = $"{GetType()}.Initialize(). Drawing is disabled. Fix errors and reload domain";
                    
                    BDebug.LogException(this, e, message);
                    
                    _gotException = true;
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

        private void OnDestroy()
        {
            Dispose();
        }
    }
}