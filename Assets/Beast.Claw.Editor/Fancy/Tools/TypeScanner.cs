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
                    .ToList();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public static void SearchTypes(Type type, string name, List<FancyTypeInfo> result)
        {
            IEnumerable<FancyTypeInfo> cash = GetCash(type);
            name = name.ToLowerInvariant();
            result.Clear();
            result.AddRange(cash.Where(typeInfo => typeInfo.nameShortLowerInvariant.Contains(name)));
        }

        private static IEnumerable<FancyTypeInfo> GetCash(Type baseType)
        {
            if (Cash.TryGetValue(baseType, out List<FancyTypeInfo> result))
            {
                return result;
            }
            
            IEnumerable<Type> derivedTypes = AllTypes.Where(baseType.IsAssignableFrom);
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
