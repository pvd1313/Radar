using UnityEditor;

namespace Beast.Claw.Attributes
{
    public class BeastDomainInitializeAttribute
    
    #if UNITY_EDITOR
    : InitializeOnLoadAttribute
    #endif
    
    {
        
    }
}