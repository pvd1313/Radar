namespace Beast.Claw.Common
{
    public static class BAssert
    {
        public static void ArgumentNotNull<T>(T obj, string argumentName)
        {
            if (obj == null)
            {
                throw new BeastAssertionException($"'{argumentName}' can not be null");
            }
        }

        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
            {
                throw new BeastAssertionException(message);
            }
        }
    }
}