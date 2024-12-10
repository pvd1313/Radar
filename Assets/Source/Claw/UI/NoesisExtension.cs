using System;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Validation;

namespace Beast.Claw.UI
{
    [ExtensionClass]
    public static class NoesisExtension
    {
        public static void Instantiate<T>(this NoesisXaml xaml, out T control) where T : BeastControl
        {
            xaml.ValidateNotNullThrowException(nameof(xaml));
            
            object obj = xaml.Load();
            
            if (obj == null)
            {
                throw new Exception($"Failed to load xaml '{xaml.uri}'");
            }

            if (obj is not T target)
            {
                throw new InvalidOperationException(
                    $"'{xaml.uri}' ({obj.GetType().FullName}) can not be casted to '{typeof(T).FullName}'");
            }

            target.uri = xaml.uri;
            target.BindElements();

            control = target;
        }
    }
}