using System;
using System.Collections.Generic;
using Beast.Claw.Attributes;
using Zenject;

namespace Beast.Claw.Common
{
    [FancyAsset]
    public abstract class ScriptableInstaller : ScriptableObjectInstaller
    {
        public List<ScriptableConfig> configs;
        
        public override void InstallBindings()
        {
            foreach (ScriptableConfig config in configs)
            {
                Container.BindInterfacesAndSelfTo(config.GetType()).FromInstance(config).AsSingle();
            }

            BindCustom(Container);
            
            IEnumerable<Type> interfaceContract = BindInterfaces();
            foreach (Type logicType in interfaceContract)
            {
                Container.BindInterfacesTo(logicType).AsSingle();
            }
        }

        protected abstract IEnumerable<Type> BindInterfaces();

        protected abstract void BindCustom(DiContainer diContainer);
    }
}