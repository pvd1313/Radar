namespace Beast.Claw
{
    public interface IValidatable
    {
        public bool IsValid(out string errorMessage);
    }
}