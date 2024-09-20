using System;
using System.Collections.Generic;
using RisenBeast.Coroutines;
using RisenBeast.Debugging;
using UnityEngine;
using Zenject;

namespace RisenBeast.Core
{
    public class CoreInstaller : ScriptableObjectInstaller
    {
        public List<ScriptableObject> configs;

        public override void InstallBindings()
        {
            Type[] controllers = 
            {
                typeof(CoroutineController),
                typeof(CoreController),
                typeof(GizmosController)
            };
            
            foreach (ScriptableObject config in configs)
            {
                Container.BindInterfacesAndSelfTo(config.GetType()).FromInstance(config).AsSingle();
            }

            foreach (Type controller in controllers)
            {
                Container.BindInterfacesAndSelfTo(controller).AsSingle();
            }
        }
    }
}