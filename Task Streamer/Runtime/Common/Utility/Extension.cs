using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Utility
{
    public static class Extension
    {
        public static bool IsNotNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s) == false;
        }



        public static void ForEach<T>(this IEnumerable<T> collection, [NotNull] Action<T> action)
        {
            if (collection is null)
            {
                return;
            }

            foreach (T element in collection)
            {
                action.Invoke(element);
            }
        }


        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = false) where T : Attribute
        {
            if (provider.GetAttribute<T>(inherit) is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, out T attribute, bool inherit = false) where T : Attribute
        {
            attribute = provider.GetAttribute<T>(inherit);

            if (attribute is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit = false) where T : Attribute
        {
            return provider.GetAttributes<T>(inherit).FirstOrDefault();
        }


        public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            try
            {
                return provider.GetCustomAttributes(typeof(T), inherit)?.Cast<T>();
            }
            catch
            {
                return Array.Empty<T>();
            }
        }
    }
}