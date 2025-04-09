using System;
using Beast.Claw.Attributes;
using Beast.Claw.Reflection;
using Beast.Claw.Text;
using VContainer;

namespace Beast.Claw.Common
{
    public class ObjectFactory : IObjectFactory
    {
        [Required] private IObjectResolver _container;
        
        public TObject Create<TObject>() where TObject : new()
        {
            if (Build.IS_DEVELOPMENT)
            {
                if (typeof(TObject).Implements(UnityType.OBJECT))
                {
                    string message = $"{typeof(TObject).FullName.Q()} is UnityEngine.Object, " +
                                     $"and can not be created with {GetType().FullName.Q()}";
                    throw new InvalidOperationException(message);
                }
            }
            
            TObject obj = new();

            _container.Inject(obj);
            
            return obj;
        }
    }
}