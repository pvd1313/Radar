using System;
using System.Collections.Generic;
using Beast.Claw.Attributes;
using Zenject;

namespace Beast.Claw.Common
{
    [FancyAsset]
    public abstract class BeastInstaller : ScriptableObjectInstaller
    {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineEditorAttribute]
#endif 
        public List<BeastConfig> configs;
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.InlineEditorAttribute]
#endif 
        public List<BeastLibrary> libraries;
        
        public override void InstallBindings()
        {
            foreach (BeastLibrary library in libraries)
            {
                Container.BindInterfacesAndSelfTo(library.GetType()).FromInstance(library).AsSingle();
            }
            
            foreach (BeastConfig config in configs)
            {
                Container.BindInterfacesAndSelfTo(config.GetType()).FromInstance(config).AsSingle();
            }
            
            IEnumerable<Type> contract = CreateContract();
            
            foreach (Type logicType in contract)
            {
                Container.BindInterfacesTo(logicType).AsSingle();
            }
        }

        protected abstract IEnumerable<Type> CreateContract();
    }
}