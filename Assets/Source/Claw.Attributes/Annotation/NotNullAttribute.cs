using System;

namespace Beast.Claw.Attributes
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
    
    public sealed class NotNullAttribute : Attribute
    {
    }
}