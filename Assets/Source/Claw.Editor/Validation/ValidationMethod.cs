using System.Reflection;
using UnityEditor;

namespace Beast.Claw.Editor.Validation
{
    public struct ValidationMethod
    {
        public MethodInfo info;
        public string fullNameWithMethod;
        public string typeFullNameLowerCase;
        public MonoScript script;
        public bool hasScript;
    }
}