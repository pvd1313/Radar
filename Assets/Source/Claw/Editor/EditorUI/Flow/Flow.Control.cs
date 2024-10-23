using System;
using Beast.Claw.Debug;

namespace Beast.Claw.Editor
{
    partial class Flow
    {
        public void StagedBeginDraw(out FlowUI ui, out FlowUIResult result)
        {
            result = default;
            ui = new FlowUI(this, _rectUI);
        }
        
        public void StagedHandleException(ref FlowUI ui, ref FlowUIResult result, Exception exception)
        {
            _handleException(ref result, exception);
        }

        public void StagedEndDraw(ref FlowUI ui, ref FlowUIResult result)
        {
            result.validationFailed |= _validationFailed;

            _validationFailed = false;
            
            _undoLastCommands(ref result);
        }
        
        public void Close()
        {
            if (_isFatal)
            {
                return;
            }
            
            if (_layoutOptionCount > 0 || 
                _commands.Count > 0 || 
                _commandStackSize.Count > 0)
            {
                BDebug.LogError(this, "Flow is not clear");
            }

            _reset();
        }
        
        public void ExitGUIException()
        {
            if (_isFatal)
            {
                return;
            }

            _reset();
        }
    }
}