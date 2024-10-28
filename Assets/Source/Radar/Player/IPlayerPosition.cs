using Unity.Mathematics;

namespace Beast.Radar.Player
{
    public interface IPlayerPosition
    {
        int2 GetPosition();
        void SetPosition(int2 cellIndex);
        void Update();
    }
}