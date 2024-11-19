using Beast.Claw.Editor.Serialization;

namespace Beast.Claw.Editor.Validation
{
    public static class ValidationPreference
    {
        public static PreferenceBool MethodDisplayEnabled;

        static ValidationPreference()
        {
            PreferenceKeyFactory factory = new("f81d4fae-7dec-11d0-a765-00a0c91e6bf6");
            PreferenceKey key = factory.CreateKey(nameof(MethodDisplayEnabled));
            
            MethodDisplayEnabled = new PreferenceBool(key, false);
        }
    }
}