using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public interface IPlayerMovement
    {
        int2 GetPosition();
        void SetPosition(int2 cellIndex);
        void Move();
    }
}