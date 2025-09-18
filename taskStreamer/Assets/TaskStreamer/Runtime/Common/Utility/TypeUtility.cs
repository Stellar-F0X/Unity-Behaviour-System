#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Properties;
using Debug = UnityEngine.Debug;
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


        public static T GetAttribute<T>(this IEnumerable<Attribute> attributes) where T : Attribute
        {
            return attributes?.FirstOrDefault(a => a is T) as T;
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
        public static MonoScript GetScriptByType(Type pocoType)
        {
            if (pocoType == null)
            {
                return null;
            }

            Assembly assembly = pocoType.Assembly;
            int targetToken = pocoType.MetadataToken;
            ReadableAttribute readable = pocoType.GetAttribute<ReadableAttribute>();

            if (readable is null)
            {
                Debug.LogError("ReadableAttribute is not found. Make sure the type is marked with [Readable] attribute.");
                return null;
            }

            string path = PathUtility.CallerFilePathToUnityPath(readable.filePath);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            if (script == null)
            {
                return null;
            }

            Type scriptType = script.GetClass();

            if (scriptType?.Assembly == assembly && scriptType.MetadataToken == targetToken)
            {
                return script;
            }

            return null;
        }



        public static List<VariableHandle> TryGetFieldHandles(Type type, object targetReference)
        {
            IPropertyBag propertyBag = PropertyBag.GetPropertyBag(type);

            if (propertyBag == null)
            {
                Debug.LogError($"{type.Name} PropertyBag is not exist");
                return null;
            }

            PriorityQueue<VariableHandle> targetProperties = new PriorityQueue<VariableHandle>(PriorityOrder.Ascending);
            propertyBag.Accept(new ReadableFieldCollectorVisitor(targetProperties), ref targetReference);
            List<VariableHandle> properties = new List<VariableHandle>(targetProperties.Count);
            
            while (targetProperties.Count > 0)
            {
                properties.Add(targetProperties.Dequeue());
            }
            
            return properties;
        }



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

            if (typeCollection.Count > 1)
            {
                Debug.LogError($"There are no or too many subclasses derived from {baseType}.");
                return null;
            }

            Type implementedType = null;

            if (typeCollection.Count == 1)
            {
                implementedType = TypeUtility.CanImplementType(typeCollection[0]);
            }

            //얻어온 타입이 구현 가능한 타입인지 확인한다.
            if (implementedType != null)
            {
                return TypeUtility.CanImplementType(typeCollection[0]);
            }

            //구현이 가능한 타입이 아니라면 대체가능한 타입인지 확인하고 반환한다. (ex: custom monoBehavior => Object)
            if (TypeUtility.TryGetAlternativeType(out Type result, argumentType[0]))
            {
                return TypeUtility.CanImplementType(result);
            }

            //Debug.LogError($"No subclass found derived from {variableType}.");
            return null;
        }


        private static Type CanImplementType(Type type)
        {
            //filter abstract class
            if (UTypeUtility.CanBeInstantiated(type) == false)
            {
                Debug.LogError($"The type {type} cannot be instantiated.");
                return null;
            }
            else
            {
                return type;
            }
        }


        private static bool TryGetAlternativeType(out Type result, Type argumentType)
        {
            if (argumentType.IsEnum)
            {
                result = typeof(EnumVariable);
                return true;
            }

            result = null;
            return false;
        }
#endif
    }
}