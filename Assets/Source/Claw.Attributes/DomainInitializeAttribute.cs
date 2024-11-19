using UnityEditor;

namespace Beast.Claw.Attributes
{
    public class DomainInitializeAttribute
    
    #if UNITY_EDITOR
    : InitializeOnLoadAttribute
    #else
    : System.Attribute
    #endif
    {
        
    }
}