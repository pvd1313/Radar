using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;
using Beast.Claw.Text;
using Beast.Claw.Validation;
using UnityEditor;

namespace Beast.Claw.Editor.Validation
{
    public static class ValidationManager
    {
        private const string ASSEMBLY_FILTER = "^(Assembly-CSharp)|(Beast.*)";
        
        private const int BUFFER_CAPACITY = 128;
        
        private static readonly bool LOG_STATS = false;

        public static IReadOnlyCollection<ValidationMethod> AllMethods => METHODS.Values;

        private static readonly Stopwatch WATCH;
        private static readonly Dictionary<Type, ValidationMethod> METHODS;
        
        static ValidationManager()
        {
            try
            {
                WATCH = new Stopwatch();
                
                Type[][] typeArrays = _getAllTypes(ASSEMBLY_FILTER);
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
                
                Reflect.GetDummy(out IValidatable dummy);
                
                MethodInfo method = type.GetMethod(nameof(dummy.Validate));
                if (method == null)
                {
                    BDebug.StaticLogError($"Failed to get validation method in {type.FullName.Q()}");
                    continue;
                }
                
                string typeFullName = type.FullName;
                MonoScript script = BAsset.TryFindScript(type);
                
                builder.Clear();
                builder += typeFullName + "." + nameof(dummy.Validate) + "()";
                string fullNameWithMethod = builder.ToString();
                
                resultDictionary[type] = new ValidationMethod
                {
                    info = method,
                    fullNameWithMethod = fullNameWithMethod,
                    typeFullNameLowerCase = typeFullName?.ToLower() ?? string.Empty,
                    script = script,
                    hasScript = script != null
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