using Beast.Claw.Common;
using UnityEngine;

namespace Beast.Claw.CG
{
    public struct MaterialParameters : IBeastValidatable
    {
        public Shader shader;
        public string name;
        
        public bool IsValid(out string errorMessage)
        {
            errorMessage = null;
            
            if (shader == null)
            {
                errorMessage = "shader is null";

                return false;
            }

            return true;
        }
    }
}