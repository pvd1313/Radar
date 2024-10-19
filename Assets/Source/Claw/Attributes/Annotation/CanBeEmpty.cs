using System;

namespace Beast.Claw.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class CanBeEmpty : Attribute
    {
    }
}