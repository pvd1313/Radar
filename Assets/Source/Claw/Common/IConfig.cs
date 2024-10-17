using System.Collections.Generic;

namespace Beast.Claw.Common
{
    public interface IConfig
    {
        IEnumerable<IConfig> Bind();
    }
}