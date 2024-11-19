using System;
using System.Reflection;
using Beast.Claw.Validation;

namespace Beast.Claw.Editor.Fancy
{
    public struct TypeScannerRule : IValidatable
    {
        public Func<Assembly, bool> assemblyRule;
        public Func<Type, bool> typeRule;
        
        public string Validate()
        {
            if (assemblyRule == null)
            {
                return "assemblyRule is null";
            }

            if (typeRule == null)
            {
                return "typeRule is null";
            }

            return null;
        }
    }
}