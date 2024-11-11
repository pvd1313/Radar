namespace Beast.Radar.Input
{
    public interface IInputDevice
    {
        DragInput Drag { get; }
        ZoomInput Zoom { get; }
    }
}