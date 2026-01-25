#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Assertions;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Properties;
using Debug = UnityEngine.Debug;
using UTypeUtility = Unity.Properties.TypeUtility;

namespace TaskStreamer.Runtime.Utility
{
    public static class TypeUtility
    {
        /// <summary>
        /// 어트리뷰트 캐시. 키는 (provider, attributeType, inherit) 조합.
        /// ConcurrentDictionary로 스레드 안전성 보장.
        /// </summary>
        private readonly static ConcurrentDictionary<(ICustomAttributeProvider, Type, bool), Attribute[]> _AttributeCache = new ConcurrentDictionary<(ICustomAttributeProvider, Type, bool), Attribute[]>();

#if UNITY_EDITOR
        /// <summary>
        /// MonoScript 캐시. 에디터 전용.
        /// </summary>
        private readonly static ConcurrentDictionary<Type, MonoScript> _MonoScriptCache = new ConcurrentDictionary<Type, MonoScript>();
#endif


        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, bool inherit = false) where T : Attribute
        {
            return provider.GetAttribute<T>(inherit) != null;
        }


        public static bool HasAttribute<T>(this ICustomAttributeProvider provider, out T attribute, bool inherit = false) where T : Attribute
        {
            attribute = provider.GetAttribute<T>(inherit);
            return attribute != null;
        }


        public static T GetAttribute<T>(this ICustomAttributeProvider provider, bool inherit = false) where T : Attribute
        {
            var attributes = provider.GetAttributesCached<T>(inherit);
            return attributes.Length > 0 ? attributes[0] : null;
        }


        public static T GetAttribute<T>(this IEnumerable<Attribute> attributes) where T : Attribute
        {
            return attributes?.FirstOrDefault(a => a is T) as T;
        }


        public static IEnumerable<T> GetAttributes<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            return GetAttributesCached<T>(provider, inherit);
        }


        /// <summary>
        /// 캐시된 어트리뷰트 배열 반환. 내부 사용 전용.
        /// 반환된 배열을 수정하지 마세요.
        /// </summary>
        private static T[] GetAttributesCached<T>(this ICustomAttributeProvider provider, bool inherit) where T : Attribute
        {
            if (provider == null)
            {
                return Array.Empty<T>();
            }

            var key = (provider, typeof(T), inherit);

            if (_AttributeCache.TryGetValue(key, out var cached))
            {
                return cached as T[] ?? Array.Empty<T>();
            }

            T[] result;
            
            try
            {
                var attrs = provider.GetCustomAttributes(typeof(T), inherit);
                result = attrs?.Length > 0 ? attrs.Cast<T>().ToArray() : Array.Empty<T>();
            }
            catch
            {
                result = Array.Empty<T>();
            }

            _AttributeCache.TryAdd(key, result);
            return result;
        }


#if UNITY_EDITOR
        public static MonoScript GetScriptByType(Type pocoType)
        {
            if (pocoType == null)
            {
                return null;
            }

            // 캐시 조회
            if (_MonoScriptCache.TryGetValue(pocoType, out var cachedScript))
            {
                return cachedScript;
            }

            Assembly assembly = pocoType.Assembly;
            int targetToken = pocoType.MetadataToken;
            ReadableAttribute readable = pocoType.GetAttribute<ReadableAttribute>();

            if (readable is null)
            {
                Debug.LogError("ReadableAttribute is not found. Make sure the type is marked with [Readable] attribute.");
                _MonoScriptCache.TryAdd(pocoType, null);
                return null;
            }

            string path = PathUtility.CallerFilePathToUnityPath(readable.filePath);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            if (script == null)
            {
                _MonoScriptCache.TryAdd(pocoType, null);
                return null;
            }

            Type scriptType = script.GetClass();

            if (scriptType?.Assembly == assembly && scriptType.MetadataToken == targetToken)
            {
                _MonoScriptCache.TryAdd(pocoType, script);
                return script;
            }

            _MonoScriptCache.TryAdd(pocoType, null);
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
            propertyBag.Accept(new ReadableFieldCollector(targetProperties), ref targetReference);
            List<VariableHandle> properties = new List<VariableHandle>(targetProperties.Count);
            
            while (targetProperties.Count > 0)
            {
                properties.Add(targetProperties.Dequeue());
            }
            
            return properties;
        }



        public static Type[] OrderByNameAndFilterAbstracts(this TypeCache.TypeCollection collection)
        {
            Type[] array = collection.Where(Include).ToArray();

            if (array.Length <= 1)
            {
                return array;
            }
            else
            {
                Array.Sort(array, (a, b) => a.Name[0].CompareTo(b.Name[0]));
                return array;
            }
            
            
            bool Include(Type t)
            {
                if (t.IsAbstract || t.IsGenericType)
                {
                    return false;
                }

                if (t.GetAttribute<HideInCreationMenuAttribute>() is null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        public static T As<T>(this Object target, bool callExceptionIfConvertedObjectIsNull = true) where T : class
        {
            Assert.IsTrue(target != null);
            T converted = target as T;

            if (callExceptionIfConvertedObjectIsNull)
            {
                Assert.IsTrue(converted != null);
            }

            return converted;
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
                result = typeof(BlackboardVariable<Enum>);
                return true;
            }

            result = null;
            return false;
        }
#endif
    }
}