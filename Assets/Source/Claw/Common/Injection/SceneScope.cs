using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Beast.Claw.Common
{
    public class SceneScope : LifetimeScope
    {
        [SerializeField] private ScriptableInstaller[] _installers;

        protected override void Configure(IContainerBuilder container)
        {
            foreach (ScriptableInstaller installer in _installers)
            {
                installer.Configure(container);
            }

            container.Register<ObjectFactory>(Lifetime.Scoped).AsImplementedInterfaces();
        }
    }
}