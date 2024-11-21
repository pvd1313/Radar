using System;

namespace Beast.Claw.Attributes.Annotation
{
    [AttributeUsage(AttributeTargets.Class | 
                    AttributeTargets.Method | 
                    AttributeTargets.Property | 
                    AttributeTargets.Field | 
                    AttributeTargets.Event |
                    AttributeTargets.Interface | 
                    AttributeTargets.Parameter | 
                    AttributeTargets.Delegate | 
                    AttributeTargets.GenericParameter)]
    
    public sealed class NotNullAttribute : AnnotationAttribute
    {
    }
}