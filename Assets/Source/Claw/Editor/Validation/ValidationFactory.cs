using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Beast.Claw.Common;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;

namespace Beast.Claw.Editor.Validation
{
    internal static class ValidationFactory
    {
        private class ValidatableDummy : IValidatable
        {
            public string Validate()
            {
                return null;
            }
        }
        
        private const string ASSEMBLY_NAME_FILTER = @"^(Assembly-CSharp)|(Beast.*)";
        
        private static readonly Dictionary<Type, ValidationMethod> VALIDATORS;
        private static readonly ValidatableDummy VALIDATION_DUMMY;

        static ValidationFactory()
        {
            try
            {
                VALIDATORS = new Dictionary<Type, ValidationMethod>();
                VALIDATION_DUMMY = new ValidatableDummy();
                
                Initialize();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        private static void Initialize()
        {
            List<Type> allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => Regex.IsMatch(a.FullName, ASSEMBLY_NAME_FILTER))
                .SelectMany(a => a.GetTypes()).ToList();

            Type validatable = typeof(IValidatable);

            foreach (Type type in allTypes)
            {
                if (!type.Implements(validatable))
                {
                    continue;
                }

                if (type.IsAbstract)
                {
                    string message = $"{type.FullName.Q()} can not be validated, because it is abstract.";
                    BDebug.StaticLogError(message);
                    
                    continue;
                }
                
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
                ConstructorInfo constructor = type.GetConstructor(flags, null, Type.EmptyTypes, null);
                
                if (constructor is null)
                {
                    BDebug.StaticLogError($"Failed to get parameterless constructor in type {type.FullName.Q()}");
                    
                    continue;
                }

                try
                {
                    ValidationMethod method = new ()
                    {
                        method = type.GetMethod(nameof(VALIDATION_DUMMY.Validate)),
                        validator = constructor.Invoke(null)
                    };
                    
                    if (method.method == null)
                    {
                        BDebug.StaticLogError($"Failed to get validation method in {type.FullName.Q()}");
                            
                        continue;
                    }
                    
                    VALIDATORS[type] = method;
                }
                catch (Exception e)
                {
                    BDebug.StaticLogError($"Failed to get validation method in {type.FullName.Q()}");
                    
                    BDebug.LogException(e);
                }
            }
        }

        public static bool TryFindValidator(Type type, out ValidationMethod validator)
        {
            return VALIDATORS.TryGetValue(type, out validator);
        }
    }
}