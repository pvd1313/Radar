using Beast.Claw.Validation;

namespace Beast.Claw.Editor.UI
{
    public interface IRectDrawTask : IValidatable
    {
        void SetStyle(UIStyleGroup style);
    }
}