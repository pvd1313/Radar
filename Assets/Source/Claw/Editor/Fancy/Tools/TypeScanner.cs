using System;
using System.Collections.Generic;
using System.Linq;
using Beast.Claw.Attributes;
using Beast.Claw.Editor.Fancy.Data;
using UnityEngine;

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
                Debug.LogException(e);
            }
        }

        public static void SearchTypes(Type type, string name, List<FancyTypeInfo> result, params Type[] excludeTypes)
        {
            IEnumerable<FancyTypeInfo> cash = GetCash(type, excludeTypes);
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
                // TODO: Exclude types is not working : https://risenbeast.youtrack.cloud/issues?preview=R-24
                .Where(type => !excludeTypes.Any(type.IsAssignableFrom));
            
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
