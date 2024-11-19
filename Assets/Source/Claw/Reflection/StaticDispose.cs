using System;
using System.Collections.Generic;
using Beast.Claw.Debug;
using Beast.Claw.Validation;
using UnityEditor;

namespace Beast.Claw.Reflection
{
    public static class StaticDispose
    {
        private static readonly List<Action> ACTIONS;

        static StaticDispose()
        {
            try
            {
                ACTIONS = new List<Action>(64);

                AssemblyReloadEvents.beforeAssemblyReload += _dispose;
            }
            catch (Exception e)
            {
                BDebug.LogException(e);
            }
        }
        
        public static void Add(Action action)
        {
            action.ValidateNotNullThrowException(nameof(action));

            ACTIONS.Add(action);
        }

        private static void _dispose()
        {
            foreach (Action action in ACTIONS)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    BDebug.LogException(e);
                }
            }
            
            ACTIONS.Clear();
        }
    }
}