using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaskStreamer.Injection;
using Unity.Properties;
using UnityEngine;
using UTypeUtility = Unity.Properties.TypeUtility;

namespace TaskStreamer.Utility
{
    public static class TypeUtility
    {
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
        
        
#if UNITY_EDITOR
        public static List<object> TryGetFieldProperties(Type type, object targetReference)
        {
            IPropertyBag propertyBag = PropertyBag.GetPropertyBag(type);

            if (propertyBag == null)
            {
                Debug.LogError($"{type.Name} PropertyBag is not exist");
                return null;
            }
            
            List<object> properties = new List<object>();
            propertyBag.Accept(new FieldCollectProcessor(properties), ref targetReference);
            return properties;
        }
        
        
        
        public static Type[] OrderByNameAndFilterAbstracts(this UnityEditor.TypeCache.TypeCollection collection)
        {
            Type[] array = collection.Where(t => t.IsAbstract == false && t.IsGenericType == false).ToArray();

            if (array.Length <= 1)
            {
                return array;
            }

            Array.Sort(array, (a, b) => a.Name[0].CompareTo(b.Name[0]));
            return array;
        }



        public static Type GetImplementedType(this Type baseType, params Type[] argumentType)
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


            UnityEditor.TypeCache.TypeCollection typeCollection = UnityEditor.TypeCache.GetTypesDerivedFrom(variableType);

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