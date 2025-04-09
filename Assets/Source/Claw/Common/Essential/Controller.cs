namespace Beast.Claw.Common
{
    public abstract class Controller : IController
    {
        public ControllerMeta Meta { get; }
        
        protected Controller()
        {
            Meta = ControllerMeta.Create(this);
        }
    }
}