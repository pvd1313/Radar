using System;
using System.Collections.Generic;
using Beast.Claw.Attributes;
using Zenject;

namespace Beast.Claw.Common
{
    [FancyAsset]
    public abstract class ScriptableInstaller : ScriptableObjectInstaller
    {
        private const int MAX_CONFIG_DEPTH = 16;
        
        public List<ScriptableConfig> configs;
        
        public override void InstallBindings()
        {
            foreach (ScriptableConfig config in configs)
            {
                _bindConfigSelfAndChildrenRecursive(config, 0);
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

        private void _bindConfigSelfAndChildrenRecursive(IConfig config, int depth)
        {
            if (depth < 0)
            {
                throw new ArgumentException(nameof(depth));
            }
            
            if (depth > MAX_CONFIG_DEPTH)
            {
                throw new Exception($"Failed to bind config. Max depth reached. {config.GetType().FullName}");
            }
            
            ValidateConfigTypeAndReference(config);

            Container.BindInterfacesAndSelfTo(config.GetType()).FromInstance(config).AsSingle();
            
            foreach (IConfig child in config.Bind())
            {
                ValidateConfigTypeAndReference(child);

                if (child == config)
                {
                    throw new Exception($"Config tried to bind self. {config.GetType().FullName}");
                }

                _bindConfigSelfAndChildrenRecursive(child, depth + 1);
            }
        }

        private void ValidateConfigTypeAndReference(IConfig config)
        {
#if DEVELOPMENT
            Type childType = config.GetType();
            if (!childType.IsClass)
            {
                string message = "Tried to bind not reference type into container. Config: " + config.GetType().FullName;
                throw new InvalidOperationException(message);
            }
                    
            if (config == null)
            {
                string message = "Tried to bind null into container. Config: " + config.GetType().FullName;
                throw new InvalidOperationException(message);
            }
#endif
        }
    }
}