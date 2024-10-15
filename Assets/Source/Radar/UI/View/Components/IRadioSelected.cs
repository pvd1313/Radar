namespace Beast.Radar.UI
{
    public interface IRadioSelected
    {
        bool IsSelected { get; }
        void Select();
        void SetSelectedMode();
        void SetDefaultMode();
    }
}