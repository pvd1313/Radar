using System;
using Beast.Claw.Debug;

namespace Beast.Claw.GameLoop
{
    public class ActionAbortedException : Exception
    {
        private static readonly ActionAbortedException EMPTY_METHOD;
        
        static ActionAbortedException()
        {
            try
            {
                EMPTY_METHOD = new ActionAbortedException("Method contains no instructions");
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        public static ActionAbortedException FromReason(EActionAbortReason reason)
        {
            if (reason == EActionAbortReason.EmptyMethod)
            {
                throw EMPTY_METHOD;
            }

            throw new Exception("Method was aborted with unknown reason: " + reason);
        }

        private ActionAbortedException(string reason) : base (reason)
        {
            
        }
    }
}