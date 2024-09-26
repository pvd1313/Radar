namespace Beast.Claw.Common
{
    public interface IValidatable
    {
        public bool IsValid(out string errorMessage);
    }
}