using System;
using Beast.Claw.Common;
using Beast.Claw.Text;
using Beast.Claw.Validation;

namespace Beast.Claw.Editor.Serialization
{
    public struct PreferenceKeyFactory
    {
        private const string EXAMPLE_GUID = "123e4567-e89b-12d3-a456-426614174000";
        private const int WORD_SIZE = 32;
        
        private static readonly int KEY_SIZE_MAX;
        
        private readonly string _guid;
        private readonly bool _isCreated;
        
        private StringBuilder _builder;

        static PreferenceKeyFactory()
        {
            KEY_SIZE_MAX = EXAMPLE_GUID.Length + WORD_SIZE;
        }

        /// <summary>
        /// Example guid: "123e4567-e89b-12d3-a456-426614174000"
        /// </summary>
        public PreferenceKeyFactory(string factoryGuid)
        {
            factoryGuid.ValidateNotNullThrowException(nameof(factoryGuid));
            
            if (!Guid.TryParse(factoryGuid, out _))
            {
                throw new ArgumentException($"Failed to parse guid: {factoryGuid.Q()}");
            }

            _guid = factoryGuid;
            _builder = new StringBuilder(KEY_SIZE_MAX + 1);
            _isCreated = true;
        }
        
        /// <summary>
        /// Example key: nameof(_windowSize)
        /// </summary>
        public PreferenceKey CreateKey(string keyID)
        {
            keyID.ValidateNotEmptyOrNullThrowException(nameof(keyID));

            if (Build.IS_DEVELOPMENT)
            {
                if (!_isCreated)
                {
                    throw new ObjectDisposedException("Factory is not created.");
                }
                
                if (keyID.Length > KEY_SIZE_MAX)
                {
                    throw new ArgumentException("Too big key size: " + keyID.Q());
                }
            }

            _builder.Clear();
            _builder += _guid + "-" + keyID;

            return new PreferenceKey(_builder.ToString());
        }
    }
}