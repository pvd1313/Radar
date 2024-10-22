namespace Beast.Claw.Reflection
{
    public static class ObjectExtension
    {
        public static string TypeName<T>(this T obj)
        {
            return TypeStorage<T>.FULL_NAME;
        }
    }
}