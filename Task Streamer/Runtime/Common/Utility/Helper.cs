using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UTypeUtility = Unity.Properties.TypeUtility;


namespace TaskStreamer.Utility
{
    public static class Helper
    {
        public static void ForEach<T>([NotNull] this IEnumerable<T> array, [NotNull] Action<T> action)
        {
            if (array is null)
            {
                return;
            }
            
            foreach (T element in array)
            {
                action.Invoke(element);
            }
        }
        
        
#if UNITY_EDITOR
        public static Type[] OrderByNameAndFilterAbstracts(this TypeCache.TypeCollection collection)
        {
            Type[] array = collection.Where(t => t.IsAbstract == false && t.IsGenericType == false).ToArray();

            if (array.Length <= 1)
            {
                return array;
            }

            Array.Sort(array, (a, b) => a.Name[0].CompareTo(b.Name[0]));
            return array;
        }
        
        
        
        public static Type GetImplementedType(in Type baseType, params Type[] argumentType)
        {
            if (UTypeUtility.CanBeInstantiated(baseType))
            {
                Debug.LogError($"The type {baseType} should not be instantiable.");
                return null;
            }
            
            Type variableType = null;

            if (argumentType is null || argumentType.Length == 0)
            {
                Debug.LogError("No generic argument types were provided.");
                return null;
            }

            try
            {
                variableType = baseType.MakeGenericType(argumentType);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (variableType == null)
            {
                Debug.LogError("Failed to create a generic type from the specified base type and arguments.");
                return null;
            }

            TypeCache.TypeCollection typeCollection = TypeCache.GetTypesDerivedFrom(variableType);

            if (typeCollection.Count == 0 || typeCollection.Count > 1)
            {
                Debug.LogError($"There are no or too many subclasses derived from {baseType}.");
                return null;
            }

            Type resultType = typeCollection[0];

            //filter abstract class
            if (UTypeUtility.CanBeInstantiated(resultType) == false)
            {
                Debug.LogError($"The type {resultType} cannot be instantiated.");
                return null;
            }
            else
            {
                return typeCollection[0];
            }
        }
#endif
    }
}