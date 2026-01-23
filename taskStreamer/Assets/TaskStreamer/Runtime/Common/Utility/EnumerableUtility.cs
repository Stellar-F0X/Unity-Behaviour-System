using System;
using System.Collections.Generic;

namespace TaskStreamer.Runtime.Utility
{
    public static class EnumerableUtility
    {
        public static void Remove<T>(this IList<T> list, Func<T, bool> condition, bool once = false)
        {
            if (list.Count == 0 || list.IsReadOnly || condition is null)
            {
                return;
            }
            
            int count = list.Count;
            
            for (int index = count - 1; index >= 0; --index)
            {
                if (condition.Invoke(list[index]) == false)
                {
                    continue;
                }
                
                list.RemoveAt(index);

                if (once)
                {
                    return;
                }
            }
        }
    }
}