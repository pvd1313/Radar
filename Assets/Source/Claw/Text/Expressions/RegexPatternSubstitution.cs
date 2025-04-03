using System;
using System.Text.RegularExpressions;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Text
{
    [Serializable]
    public struct RegexPatternSubstitution : IValidatable
    {
        [SerializeField] public string pattern;
        [SerializeField] public string substitution;

        public string Replace(string text)
        {
            return Regex.Replace(text, pattern, substitution);
        }

        public string Validate()
        {
            if (pattern is null)
            {
                return $"'{nameof(pattern)}' is null";
            }

            if (substitution is null)
            {
                return $"'{nameof(substitution)}' is null";
            }

            if (!RegexPattern.RegexIsValid(nameof(pattern), pattern, out string message))
            {
                return message;
            }

            return null;
        }
    }
}