using System;
using System.Collections.Generic;
using UnityEngine;

namespace TaskStreamer.Utility
{
    public static class StringUtility
    {
        public static bool IsNotNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s) == false;
        }


        public static int StringToHash(in string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Blackboard key cannot be null or empty.");
                return -1;
            }

            return Animator.StringToHash(key);
        }


        public static bool TrySetNamesOfEnumFlag<TEnum>(TEnum value, List<string> bucket) where TEnum : Enum
        {
            Type enumType = typeof(TEnum);

            if (enumType.HasAttribute<FlagsAttribute>() == false)
            {
                return false;
            }

            foreach (TEnum element in Enum.GetValues(enumType))
            {
                if (value.HasFlag(element))
                {
                    bucket.Add(element.ToString());
                }
            }

            return true;
        }


#if UNITY_EDITOR
        public static string ToNicifyName(string nodeName, string removeName = "")
        {
            if (string.IsNullOrEmpty(nodeName))
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: NodeName is null or empty");
            }

            if (string.IsNullOrEmpty(removeName) == false && nodeName.EndsWith(removeName))
            {
                nodeName = nodeName.Replace(removeName, string.Empty);
            }

            return UnityEditor.ObjectNames.NicifyVariableName(nodeName);
        }
#endif
    }
}