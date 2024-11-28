using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public interface IWindow<in TData> where TData : struct
    {
        UniTask Open(TData data);

        UniTask Close();
    }
}