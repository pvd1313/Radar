using Beast.Claw.Common;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Graphic
{
    public struct MaterialParameters : IValidatable
    {
        public Shader shader;
        public string name;
        
        public string Validate()
        {
            if (shader == null)
            {
                return "shader is null";
            }

            return null;
        }
    }
}