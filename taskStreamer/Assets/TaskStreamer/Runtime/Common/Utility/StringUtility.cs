using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TaskStreamer.Runtime.Utility
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
                throw new ArgumentException($"{typeof(TSObjectFactory)}: NodeName is null or empty");
            }

            if (string.IsNullOrEmpty(removeName) == false && nodeName.EndsWith(removeName))
            {
                nodeName = nodeName.Replace(removeName, string.Empty);
            }

            return UnityEditor.ObjectNames.NicifyVariableName(nodeName);
        }


        public static bool IsValidScriptName(string scriptName, out string errorMessage)
        {
            if (string.IsNullOrEmpty(scriptName))
            {
                errorMessage = "Node file name is required.";
                return false;
            }
			
            if (char.IsDigit(scriptName[0]) || scriptName[0] == '_')
            {
                errorMessage = "Node file name cannot start with a number or an underscore.";
                return false;
            }

            if (scriptName.Contains(" "))
            {
                errorMessage = "Node file name cannot contain spaces.";
                return false;
            }
			
            if (scriptName.All(c => c == '_' || char.IsLetterOrDigit(c)))
            {
                errorMessage = null;
                return true;
            }
            else
            {
                errorMessage = "Node file name must contain only letters, numbers and underscores.";
            }

            return false;
        }
#endif
    }
}