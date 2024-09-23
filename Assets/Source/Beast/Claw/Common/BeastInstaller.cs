using System;
using System.Collections.Generic;
using Zenject;

namespace Beast.Claw.Common
{
    public abstract class BeastInstaller : ScriptableObjectInstaller
    {
        public List<BeastConfig> configs;
        
        public override void InstallBindings()
        {
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