using System;
using Beast.Claw.Attributes.Annotation;
using UnityEngine;

namespace Beast.Claw.Reflection
{
    [ConstantGroup]
    public static class UnityType
    {
        public static readonly Type GAME_OBJECT = typeof(GameObject);
        public static readonly Type MONO_BEHAVIOUR = typeof(MonoBehaviour);
    }
}