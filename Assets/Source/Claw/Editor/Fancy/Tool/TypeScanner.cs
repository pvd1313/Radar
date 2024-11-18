using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.Reflection;
using Beast.Claw.Text;

namespace Beast.Claw.Editor.Fancy
{
    internal static class TypeScanner
    {
        private static readonly Dictionary<Type, List<FancyTypeInfo>> CASH;
        private static readonly List<Type> ALL_TYPES;

        static TypeScanner()
        {
            try
            {
                CASH = new Dictionary<Type, List<FancyTypeInfo>>();

                ALL_TYPES = 
                    AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsDefined(typeof(FancyAssetAttribute), true))
                    .Where(type => !type.IsAbstract)
                    .ToList();
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }

        public static void SearchTypes(Type[] types, StringWithFormat lowercaseName, List<FancyTypeInfo> result, Type[] excludeTypes)
        {
            if (lowercaseName.Format != EStringFormat.LowerCase)
            {
                throw new ArgumentException("Expected lower case name, but was: " + lowercaseName.Format.Q());
            }
            
            result.Clear();

            foreach (Type type in types)
            {
                IReadOnlyList<FancyTypeInfo> perTypeCash = GetCash(type, excludeTypes);

                foreach (FancyTypeInfo info in perTypeCash)
                {
                    if (info.nameShortLowerInvariant.Contains(lowercaseName))
                    {
                        result.Add(info);
                    }
                }
            }
        }

        private static IReadOnlyList<FancyTypeInfo> GetCash(Type baseType, Type[] excludeTypes)
        {
            if (CASH.TryGetValue(baseType, out List<FancyTypeInfo> result))
            {
                return result;
            }
            
            IEnumerable<Type> derivedTypes = ALL_TYPES
                .Where(type => type.Implements(baseType))
                .Where(type => !excludeTypes.Any(excludeType => type.Implements(excludeType)));
            
            result = new List<FancyTypeInfo>();
            
            foreach (Type type in derivedTypes)
            {
                FancyTypeInfo info = new()
                {
                    type = type,
                    nameShort = type.Name,
                    theNamespace = type.Namespace,
                    nameShortLowerInvariant = type.Name.ToLowerInvariant()
                };
 
                result.Add(info);
            }
            
            CASH[baseType] = result;
            
            return result;
        }
    }
}
