using System;
using Beast.Claw.Validation;
using Noesis;

namespace Beast.Claw.UI
{
    public abstract class BeastControl : UserControl
    {
        public string uri;
        
        public abstract void BindElements();

        protected void _find<T>(string name, out T element) where T : FrameworkElement
        {
            name.ValidateNotEmptyOrNullThrowException(nameof(name));

            object obj = FindName(name);
            
            if (obj == null)
            {
                throw new InvalidOperationException($"'{name}' was not found in '{uri}'");
            }

            if (obj is not T target)
            {
                throw new InvalidOperationException(
                    $"'{name}' ({obj.GetType().FullName}) can not be casted to ({typeof(T).FullName})");
            }

            element = target;
        }
    }
}