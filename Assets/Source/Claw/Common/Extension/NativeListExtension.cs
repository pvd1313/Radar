using Beast.Claw.Attributes.Annotation;
using Unity.Collections;

namespace Beast.Claw.Common
{
    [ExtensionClass]
    public static class NativeListExtension
    {
        public static void Reverse<T>(this INativeList<T> list) where T : unmanaged
        {
            int count = list.Length;
            
            for (int i = 0; i < count / 2; i++)
            {
                (list[i], list[count - i - 1]) = (list[count - i - 1], list[i]);
            }
        }
    }
}