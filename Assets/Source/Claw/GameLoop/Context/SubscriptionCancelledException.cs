using System;
using Beast.Claw.Debug;

namespace Beast.Claw.GameLoop
{
    public class SubscriptionCancelledException : Exception
    {
        public static SubscriptionCancelledException EmptyMethod { get; }
        
        static SubscriptionCancelledException()
        {
            try
            {
                EmptyMethod = new SubscriptionCancelledException("Method contains no instructions");
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
        public SubscriptionCancelledException(string reason) : base (reason)
        {
            
        }
    }
}