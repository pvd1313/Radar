using System;

namespace Beast.Claw.Attributes.Annotation
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field)]
    public sealed class CanBeNullAttribute : AnnotationAttribute
    {
        
    }
}