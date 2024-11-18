using System;

namespace Beast.Claw.Validation
{
    public static class Validate
    {
        public static bool Regex(string propertyName, string regex, out string message)
        {
            message = string.Empty;
            
            try
            {
                System.Text.RegularExpressions.Regex.IsMatch("", regex);
            }
            catch (Exception e)
            {
                message = $"'{propertyName}' is invalid: {e.Message}";
                return false;
            }

            return true;
        }
    }
}