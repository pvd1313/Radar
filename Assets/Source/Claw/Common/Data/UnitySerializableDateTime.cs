using System;
using Beast.Claw.Text;
using Beast.Claw.Validation;
using UnityEngine;

namespace Beast.Claw.Common
{
    [Serializable]
    public struct UnitySerializableDateTime : ISerializationCallbackReceiver, IValidatable
    {
        public DateTime Date => _date;
        
        [SerializeField] private string _dateString;
        
        private DateTime _date;
        
        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            DateTime.TryParse(_dateString, out _date);
        }

        public string Validate()
        {
            if (!DateTime.TryParse(_dateString, out DateTime _))
            {
                return "Failed to parse date " + _dateString.Q();
            }

            return null;
        }
    }
}