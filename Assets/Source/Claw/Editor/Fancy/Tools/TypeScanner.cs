using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Attributes;
using Beast.Claw.Debug;
using Beast.Claw.Editor.Fancy.Data;

namespace Beast.Claw.Editor.Fancy.Tools
{
    internal static class TypeScanner
    {
        private static readonly Dictionary<Type, List<FancyTypeInfo>> Cash;
        private static readonly List<Type> AllTypes;

        static TypeScanner()
        {
            try
            {
                Cash = new Dictionary<Type, List<FancyTypeInfo>>();
                AllTypes = 
                    AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsDefined(typeof(FancyAssetAttribute), true))
                    .Where(type => !type.IsAbstract)
                    .ToList();
            }
            catch (Exception e)
            {
                BDebug.StaticLogException(e);
            }
        }

        public static void SearchTypes(Type[] types, string name, List<FancyTypeInfo> result, params Type[] excludeTypes)
        {
            List<FancyTypeInfo> cash = new List<FancyTypeInfo>();
            foreach (var type in types)
            {
                cash.AddRange(GetCash(type, excludeTypes));
            }
            name = name.ToLowerInvariant();
            result.Clear();
            result.AddRange(cash.Where(typeInfo => typeInfo.nameShortLowerInvariant.Contains(name)));
        }

        private static IEnumerable<FancyTypeInfo> GetCash(Type baseType, params Type[] excludeTypes)
        {
            if (Cash.TryGetValue(baseType, out List<FancyTypeInfo> result))
            {
                return result;
            }
            
            IEnumerable<Type> derivedTypes = AllTypes
                .Where(baseType.IsAssignableFrom)
                .Where(type => !excludeTypes.Any(excludeType => excludeType.IsAssignableFrom(type)));
            
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
            
            Cash[baseType] = result;
            
            return result;
        }
    }
}
