using System;
using VContainer;

namespace Beast.Claw.Attributes
{
    /// <summary>
    /// Reference will be injected by DI container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : InjectAttribute
    {
        
    }
}