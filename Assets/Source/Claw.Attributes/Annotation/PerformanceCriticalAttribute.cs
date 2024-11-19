using System;

namespace Beast.Claw.Attributes.Annotation
{
    /// <summary>
    /// Method is slow and should not be used in main loop. Game loop, editor draw loop, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PerformanceCriticalAttribute : Attribute
    {
        
    }
}