using Beast.Claw.Common;

using Color = UnityEngine.Color;

namespace Beast.Claw.Gizmos
{
    public interface IGizmosManager
    {
        GizmosHandle GetHandle(IController owner, Color defaultColor);
        GizmosHandle GetHandle(IController owner, string handleName, Color defaultColor);
    }
}