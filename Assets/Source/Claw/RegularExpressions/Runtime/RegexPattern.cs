using System;
using System.Text.RegularExpressions;

namespace Beast.Claw.RegularExpressions
{
    [Serializable]
    public struct RegexPattern
    {
        public string pattern;

        public bool IsMatch(string text)
        {
            return Regex.IsMatch(text, pattern);
        }
    }
}