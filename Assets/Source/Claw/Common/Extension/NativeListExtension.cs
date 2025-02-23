using Beast.Claw.Attributes.Annotation;
using PlasticPipe.PlasticProtocol.Messages;
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

        public static void AddRangeReverseOrder<T>(this NativeList<T> list, NativeList<T> source) where T : unmanaged
        {
            for (int i = source.Length - 1; i >= 0; i--)
            {
                list.Add(source[i]);
            }
        }

        public static bool RemoveValue<T>(this NativeList<T> list, T value) where T : unmanaged
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].Equals(value))
                {
                    list.RemoveAt(i);
                    
                    return true;
                }
            }

            return false;
        }
    }
}