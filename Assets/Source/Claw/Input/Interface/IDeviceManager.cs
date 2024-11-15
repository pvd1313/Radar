namespace Beast.Claw.Input
{
    public interface IDeviceManager
    {
        public IInputDevice ActiveDevice { get; }
    }
}