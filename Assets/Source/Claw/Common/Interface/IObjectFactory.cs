namespace Beast.Claw.Common
{
    public interface IObjectFactory
    {
        /// <summary>
        /// Creates TObject and injects dependencies.
        /// </summary>
        public TObject Create<TObject>() where TObject : new();
    }
}