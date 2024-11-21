using System.Collections.Generic;
using Beast.Claw.Attributes.Annotation;

namespace Beast.Claw.Editor
{
    [ExtensionClass]
    public static class ListExtension
    {
        [PerformanceCritical]
        public static bool TryMove<TValue>(this IList<TValue> list, int sourceIndex, int targetIndex)
        {
            if (list.Count <= 1)
            {
                return false;
            }

            if (sourceIndex == targetIndex)
            {
                return false;
            }

            int lastIndex = list.Count - 1;
            if (targetIndex < 0 || sourceIndex < 0 || sourceIndex > lastIndex || targetIndex > lastIndex)
            {
                return false;
            }

            list.Insert(targetIndex, list[sourceIndex]);
            
            if (targetIndex < sourceIndex)
            {
                list.RemoveAt(sourceIndex + 1);
            }
            else
            {
                list.RemoveAt(sourceIndex);
            }

            return true;
        }
    }
}