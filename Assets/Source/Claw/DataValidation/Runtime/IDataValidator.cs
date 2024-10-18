namespace Beast.Claw.DataValidation
{
    public interface IDataValidator<T>
    {
        public string Validate(T data);
    }
}