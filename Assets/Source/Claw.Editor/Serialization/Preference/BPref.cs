using System;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Editor.Serialization
{
    [StaticUtility]
    public static class BPref
    {
        public static event Action Changed;
        
        public static void ReadFloat(PreferenceKey key, out float value, float defaultValue)
        {
            key.ValidateThrowException(nameof(key));

            value = PlayerPrefs.GetFloat(key.value, defaultValue);
        }

        public static void WriteFloat(PreferenceKey key, float value)
        {
            key.ValidateThrowException(nameof(key));

            PlayerPrefs.SetFloat(key.value, value);
            
            _onPreferenceChanged();
        }

        public static void ReadBool(PreferenceKey key, out bool value, bool defaultValue)
        {
            key.ValidateThrowException(nameof(key));

            int valueInt = PlayerPrefs.GetInt(key.value, defaultValue ? 1 : 0);

            value = valueInt != 0;
        }

        public static void WriteBool(PreferenceKey key, bool value)
        {
            key.ValidateThrowException(nameof(key));

            PlayerPrefs.SetInt(key.value, value ? 1 : 0);
            
            _onPreferenceChanged();
        }

        private static void _onPreferenceChanged()
        {
            BAsset.RepaintEditorWindows();
            
            Changed?.Invoke();
        }
        
    }
}