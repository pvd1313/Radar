using System;
using System.Collections.Generic;
using System.Reflection; 
using Beast.Claw.Reflection;
using Beast.Claw.Text;
using Beast.Claw.Validation;

namespace Beast.Claw.Editor.Fancy
{
    public class TypeScanner
    {
        private readonly List<TypeData> _allTypes;
        private readonly List<TypeData> _filteredTypes;

        public TypeScanner(TypeScannerRule rule)
        {
            rule.ValidateThrowException(nameof(rule));

            _allTypes = new List<TypeData>(64);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                if (!rule.assemblyRule.Invoke(assembly))
                {
                    continue;
                }

                Type[] assemblyTypes = assembly.GetTypes();

                foreach (Type type in assemblyTypes)
                {
                    if (!rule.typeRule.Invoke(type))
                    {
                        continue;
                    }
                    
                    _allTypes.Add(new TypeData
                    {
                        type = type,
                        nameShort = type.Name,
                        nameShortLowerCase = type.Name.ToLower()
                    });
                }
            }

            _filteredTypes = new List<TypeData>(_allTypes.Count);
        }

        public IReadOnlyList<TypeData> Filter(StringWithFormat lowerCaseName)
        {
            if (lowerCaseName.Format != EStringFormat.LowerCase)
            {
                throw new ArgumentException("Expected lower case name, but was: " + lowerCaseName.Format.Q());
            }

            if (lowerCaseName.Value.IsNullOrEmpty())
            {
                return _allTypes;
            }
            
            _filteredTypes.Clear();

            foreach (TypeData typeData in _allTypes)
            {
                if (typeData.nameShortLowerCase.Contains(lowerCaseName))
                {
                    _filteredTypes.Add(typeData);
                }
            }

            return _filteredTypes;
        }
    }
}
