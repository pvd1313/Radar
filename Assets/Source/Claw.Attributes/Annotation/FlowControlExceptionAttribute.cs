using System;

namespace Beast.Claw.Attributes.Annotation
{
    /// <summary>
    /// Exception, that is used to prevent further logic execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FlowControlExceptionAttribute : Attribute
    {
        
    }
}