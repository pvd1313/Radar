using System;
using Beast.Claw.Text;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Reflection
{
    [Serializable]
    public struct TypeAddress : IValidatable
    {
        private static StringBuilder _stringBuilder = new (256);
        
        public string assembly;
        
        [Multiline]
        public string type;
        
        public string Validate()
        {
            if (assembly.IsNullOrEmpty())
            {
                return "assembly is empty";
            }

            if (type.IsNullOrEmpty())
            {
                return "type is empty";
            }

            if (TryGetType() == null)
            {
                StringBuilder str = _stringBuilder.Clear();
                str = str.Clear() + "Type '" + type + "' was not found in '" + assembly + "'";

                return str.ToString();
            }

            return null;
        }

        public Type TryGetType()
        {
            StringBuilder str = _stringBuilder.Clear();
            str = str + type + ", " + assembly;

            return Type.GetType(str.ToString());
        }
    }
}