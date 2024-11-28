using System;
using System.Collections.Generic;
using System.Diagnostics;
using Beast.Claw.Attributes;
using VContainer;

namespace Beast.Claw.Common
{
    [FancyAsset]
    public abstract class ScriptableInstaller : Scriptable
    {
        private const int MAX_CONFIG_DEPTH = 16;
        
        public List<ScriptableConfig> configs;

        public void Configure(IContainerBuilder container)
        {
            foreach (IConfig config in _bindConfigs())
            {
                _bindConfigSelfAndChildrenRecursive(container, config, 0);
            }
            
            foreach (ScriptableConfig config in configs)
            {
                _bindConfigSelfAndChildrenRecursive(container, config, 0);
            }
            
            IEnumerable<Type> interfaceContract = _bindInterfaces();
            foreach (Type logicType in interfaceContract)
            {
                container.Register(logicType, Lifetime.Singleton).AsImplementedInterfaces();
            }

            foreach (Type model in _bindModels())
            {
                container.Register(model, Lifetime.Singleton).AsSelf();
            }
        }

        protected abstract IEnumerable<IConfig> _bindConfigs();

        protected abstract IEnumerable<Type> _bindInterfaces();

        protected abstract IEnumerable<Type> _bindModels();

        private void _bindConfigSelfAndChildrenRecursive(IContainerBuilder container, IConfig config, int depth)
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

            container.RegisterInstance(config).AsSelf();

            foreach (IConfig child in config.Bind())
            {
                ValidateConfigTypeAndReference(child);

                if (child == config)
                {
                    throw new Exception($"Config tried to bind self. {config.GetType().FullName}");
                }

                _bindConfigSelfAndChildrenRecursive(container, child, depth + 1);
            }
        }

        [Conditional("DEVELOPMENT")]
        private void ValidateConfigTypeAndReference(IConfig config)
        {
            if (config == null)
            {
                string message = $"Tried to bind null into container. Installer: {GetType().FullName}";
                throw new InvalidOperationException(message);
            }
            
            Type childType = config.GetType();
            if (!childType.IsClass)
            {
                string message = "Tried to bind not reference type into container. Config: " + config.GetType().FullName;
                throw new InvalidOperationException(message);
            }
        }
    }
}