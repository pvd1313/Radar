using System;
using Beast.Claw.Attributes.Annotation;
using Beast.Claw.Validation;
using Noesis;
using UnityEngine;
using Color = Noesis.Color;

namespace Beast.Claw.UI
{
    [ExtensionClass]
    public static class NoesisExtension
    {
        public static void Instantiate<T>(this NoesisXaml xaml, out T control) where T : BeastControl
        {
            xaml.ValidateNotNullThrowException(nameof(xaml));
            
            object obj = xaml.Load();
            
            if (obj == null)
            {
                throw new Exception($"Failed to load xaml '{xaml.uri}'");
            }

            if (obj is not T target)
            {
                throw new InvalidOperationException(
                    $"'{xaml.uri}' ({obj.GetType().FullName}) can not be casted to '{typeof(T).FullName}'");
            }

            target.uri = xaml.uri;
            target.BindElements();

            control = target;
        }

        public static Color ToNoesis(this UnityEngine.Color color)
        {
            return new Color()
            {
                A = (byte)(color.a * 255),
                R = (byte)(color.r * 255),
                G = (byte)(color.g * 255),
                B = (byte)(color.b * 255),
            };
        }

        public static void SetMaterial(this Rectangle rectangle, Material material)
        {
            TextureSource textureSource = new(material.mainTexture);
            ImageBrush brush = new(textureSource);
            rectangle.Fill = brush;
        }
    }
}