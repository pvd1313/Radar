using Beast.Claw.Common;
using Beast.Claw.Validation;

namespace Beast.Claw.Editor
{
    public interface IRectDrawTask : IValidatable
    {
        void SetStyle(UIStyleGroup style);
    }
}