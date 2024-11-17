using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Beast.Claw.Common;
using Beast.Claw.Common.Text;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;

namespace Beast.Claw.Editor.Validation
{
    internal static class ValidationMethodStorage
    {
        private class ValidatableDummy : IValidatable
        {
            public string Validate()
            {
                return null;
            }
        }
        
        private const string ASSEMBLY_NAME_FILTER = "^(Assembly-CSharp)|(Beast.*)";
        private const int BUFFER_CAPACITY = 128;
        
        private static readonly bool LOG_STATS = false;
        
        private static readonly ValidatableDummy DUMMY;
        private static readonly Stopwatch WATCH;
        private static readonly Dictionary<Type, ValidationMethod> METHODS;
        
        static ValidationMethodStorage()
        {
            try
            {
                DUMMY = new ValidatableDummy();
                WATCH = new Stopwatch();
                
                Type[][] typeArrays = _getAllTypes(ASSEMBLY_NAME_FILTER);
                METHODS = _createValidationMethods(typeArrays);
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        private static Type[][] _getAllTypes(string assemblyNameRegex)
        {
            WATCH.Restart();
            
            Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> filteredAssemblies = new (allAssemblies.Length);

            foreach (Assembly assembly in allAssemblies)
            {
                if (Regex.IsMatch(assembly.FullName, assemblyNameRegex))
                {
                    filteredAssemblies.Add(assembly);
                }
            }

            Type[][] typeArrays = new Type[filteredAssemblies.Count][];

            int typeCount = 0;
            for (int i = 0; i < filteredAssemblies.Count; i++)
            {
                Assembly assembly = filteredAssemblies[i];

                Type[] types = assembly.GetTypes();
                
                typeArrays[i] = types;
                typeCount += types.Length;
            }
            
            WATCH.Stop();

            if (LOG_STATS)
            {
                BDebug.StaticLogInfo($"_getAllTypes: {typeCount} types, {WATCH.ElapsedMillisecondsFloat()}ms");
            }

            return typeArrays;
        }

        private static Dictionary<Type, ValidationMethod> _createValidationMethods(Type[][] typeArrays)
        {
            Type validatable = typeof(IValidatable);
            StringBuilder builder = new (256);
            
            WATCH.Restart();

            Dictionary<Type, ValidationMethod> resultDictionary = new(BUFFER_CAPACITY);

            foreach (Type[] typeArr in typeArrays)
            foreach (Type type in typeArr)
            {
                if (!type.Implements(validatable) || type.IsInterface)
                {
                    continue;
                }

                if (type.IsAbstract)
                {
                    string message = $"{type.FullName.Q()} can not be validated, because it is abstract.";
                    BDebug.StaticLogError(message);
                    
                    continue;
                }
                
                MethodInfo method = type.GetMethod(nameof(DUMMY.Validate));
                if (method == null)
                {
                    BDebug.StaticLogError($"Failed to get validation method in {type.FullName.Q()}");
                    continue;
                }
                
                builder.Clear();
                builder += type.FullName + "." + nameof(DUMMY.Validate) + "()";
                
                resultDictionary[type] = new ValidationMethod
                {
                    info = method,
                    name = builder.ToString()
                };
            }
            
            WATCH.Stop();

            if (LOG_STATS)
            {
                BDebug.StaticLogInfo($"_createValidationMethods: {resultDictionary.Count} methods, " +
                                     $"{WATCH.ElapsedMillisecondsFloat()}ms");
            }

            if (BUFFER_CAPACITY < resultDictionary.Count)
            {
                BDebug.StaticLogWarning($"Buffer capacity was smaller ({BUFFER_CAPACITY}) " +
                                        $"than method count ({resultDictionary.Count})");
            }
            
            return resultDictionary;
        }

        public static bool TryFindValidator(Type type, out ValidationMethod validator)
        {
            return METHODS.TryGetValue(type, out validator);
        }

        public static void Forget(Type type)
        {
            METHODS.Remove(type);
        }
    }
}