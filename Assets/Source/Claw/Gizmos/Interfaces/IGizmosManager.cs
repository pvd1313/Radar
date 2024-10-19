using UnityEngine;

namespace Beast.Claw.Gizmos
{
    public interface IGizmosManager
    {
        GizmosHandle GetHandle(object owner, Color defaultColor);
        GizmosHandle GetHandle(object owner, string name, Color defaultColor);
    }
}