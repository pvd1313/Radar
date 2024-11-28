using Cysharp.Threading.Tasks;

namespace Beast.Radar.UI
{
    public interface IWindowHandle<in TData> where TData : struct
    {
        UniTask Open(TData data);
    }
}