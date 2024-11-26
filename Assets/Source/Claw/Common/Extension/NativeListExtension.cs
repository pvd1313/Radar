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

        public static void AddRangeReverseOrder<T>(this NativeList<T> list, NativeArray<T> source) where T : unmanaged
        {
            for (int i = source.Length - 1; i >= 0; i--)
            {
                list.Add(source[i]);
            }
        }
    }
}