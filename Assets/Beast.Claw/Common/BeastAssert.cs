namespace Beast.Claw.Common
{
    public static class BeastAssert
    {
        public static void NotNull<T>(T obj, string objectName)
        {
            if (obj == null)
            {
                throw new BeastAssertionException($"'{objectName}' can not be null");
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