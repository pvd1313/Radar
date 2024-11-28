using System.Collections.Generic;

namespace Beast.Claw.Common
{
    public abstract class ScriptableConfig : Scriptable, IConfig
    {
        public abstract IEnumerable<IConfig> Bind();
    }
}