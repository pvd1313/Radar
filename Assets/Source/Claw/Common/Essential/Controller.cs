namespace Beast.Claw.Common
{
    public class Controller : IController
    {
        public ControllerMeta Meta { get; }
        
        protected Controller()
        {
            Meta = ControllerMeta.Create(this);
        }
    }
}