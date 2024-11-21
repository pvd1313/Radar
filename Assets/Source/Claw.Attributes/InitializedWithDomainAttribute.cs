using UnityEditor;

namespace Beast.Claw.Attributes
{
    /// <see cref="InitializeOnLoadAttribute"/>
    public class InitializedWithDomainAttribute
    
    #if UNITY_EDITOR
    : InitializeOnLoadAttribute
    #else
    : System.Attribute
    #endif
    {
        
    }
}