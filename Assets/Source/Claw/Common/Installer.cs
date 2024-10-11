using System;
using System.Collections.Generic;
using Beast.Claw.Attributes;
using Zenject;

namespace Beast.Claw.Common
{
    [FancyAsset]
    public abstract class Installer : ScriptableObjectInstaller
    {
        public List<Config> configs;
        public List<Library> libraries;
        
        public override void InstallBindings()
        {
            foreach (Library library in libraries)
            {
                Container.BindInterfacesAndSelfTo(library.GetType()).FromInstance(library).AsSingle();
            }
            
            foreach (Config config in configs)
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