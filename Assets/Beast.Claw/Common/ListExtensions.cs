using System.Collections.Generic;

namespace Beast.Claw.Common
{
    public static class ListExtensions
    {
        public static T PopOrNull<T>(this IList<T> list) where T : class
        {
            if (list.Count == 0)
            {
                return null;
            }

            T item = list[^1];
            
            list.RemoveAt(list.Count - 1);

            return item;
        }
    }
}