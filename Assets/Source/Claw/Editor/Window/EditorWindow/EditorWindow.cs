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

        protected abstract void Warmup();

        protected abstract void InspectorUI();
        
        protected abstract void Dispose();

        protected EditorWindow()
        {
            AssemblyReloadEvents.afterAssemblyReload += _afterAssemblyReload;
        }

        private void _afterAssemblyReload()
        {
            AssemblyReloadEvents.afterAssemblyReload += _afterAssemblyReload;

            _initialized = false;
            _gotException = false;
            
            Dispose();
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
                    BDebug.LogException(this, e, $"{GetType()}.Initialize()." +
                                                 " Window drawing will be disabled. " +
                                                 " Fix error and reload domain.");
                    _gotException = true;
                }
            }

            try
            {
                InspectorUI();
                
                _flow.CheckFlowIsFinished();
            }
            catch (ExitGUIException)
            {
                _flow.TerminateFlow();
            }
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}