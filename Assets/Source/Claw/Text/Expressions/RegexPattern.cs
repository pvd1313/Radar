using System;
using System.Text.RegularExpressions;
using Beast.Claw.Validation;

namespace Beast.Claw.Text
{
    [Serializable]
    public struct RegexPattern : IValidatable
    {
        public string pattern;

        public bool IsMatch(string text)
        {
            return Regex.IsMatch(text, pattern);
        }

        public string Validate()
        {
            if (pattern is null)
            {
                return $"'{nameof(pattern)}' is null";
            }

            if (!RegexIsValid(nameof(pattern), pattern, out string message))
            {
                return message;
            }

            return null;
        }
        
        public static bool RegexIsValid(string propertyName, string regex, out string message)
        {
            message = string.Empty;
            
            try
            {
                Regex.IsMatch("", regex);
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