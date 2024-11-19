using Beast.Claw.Validation;

namespace Beast.Claw.Editor.Serialization
{
    public readonly struct PreferenceKey : IValidatable
    {
        public readonly string value;

        public PreferenceKey(string fromKey)
        {
            value = fromKey;
        }
        
        public string Validate()
        {
            if (string.IsNullOrEmpty(value))
            {
                return "Empty key!";
            }

            return null;
        }
    }
}