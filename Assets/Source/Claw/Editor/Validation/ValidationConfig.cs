using System.Collections.Generic;
using Beast.Claw.Editor.Serialization;
using Beast.Claw.Reflection;

namespace Beast.Claw.Editor.Validation
{
    public class ValidationConfig : ScriptableConfig
    {
        public List<TypeAddress> excludeAssetType = new ();
    }
}