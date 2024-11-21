using System;
using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Input
{
    /// <summary>
    /// Used to prevent other axis behaviour, when axis transformed into other axis. 
    /// As consumer expected to be already notified about new axis.
    /// </summary>
    [FlowControlException]
    public class AxisMutatedException : Exception
    {
        public static AxisMutatedException Default { get; } = new ();
    }
}