using Beast.Claw.Common;

namespace Beast.Claw.Gizmo
{
    public interface IGizmoManager
    {
        IGizmoHandle CreateHandle(IController owner);
    }
}