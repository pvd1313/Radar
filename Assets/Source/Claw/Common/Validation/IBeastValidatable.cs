namespace Beast.Claw.Common
{
    public interface IBeastValidatable
    {
        public bool IsValid(out string errorMessage);
    }
}