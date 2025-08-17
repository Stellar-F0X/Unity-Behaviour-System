using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using Unity.Properties;
using UnityEditor;
#endif

namespace TaskStreamer.Utility
{
    public static class TypeCollection
    {
        private readonly static Dictionary<Type, Type> _GenericTypeToVariableType = new Dictionary<Type, Type>(128);

        private readonly static List<Type> _AllVariableTypes = new List<Type>(128);

        private static Type[] _cachedVariableTypesArray;

        private static bool _isInitialized = false;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        public static void CollectTypes()
        {
            if (_isInitialized)
            {
                return;
            }

            //Stopwatch stopwatch = Stopwatch.StartNew();
            
            CollectVariableTypesOptimized();
            
            _isInitialized = true;
            
            //stopwatch.Stop();
            //Debug.Log($"TypeCollection initialized with Variable<T> types in {stopwatch.ElapsedMilliseconds}ms");
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CollectVariableTypesOptimized()
        {
            _GenericTypeToVariableType.Clear();
            _AllVariableTypes.Clear();

            Assembly[] assemblies = GetRelevantAssemblies();
            Type variableGenericType = typeof(Variable<>);

            foreach (Assembly assembly in assemblies)
            {
                ProcessAssemblyTypes(assembly, variableGenericType);
            }

            _cachedVariableTypesArray = _AllVariableTypes.ToArray();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Assembly[] GetRelevantAssemblies()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> relevantAssemblies = new List<Assembly>(assemblies.Length);

            foreach (Assembly assembly in assemblies)
            {
                if (IsRelevantAssembly(assembly.GetName().Name))
                {
                    relevantAssemblies.Add(assembly);
                }
            }

            return relevantAssemblies.ToArray();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsRelevantAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("System", StringComparison.OrdinalIgnoreCase) == false &&
                   assemblyName.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) == false &&
                   assemblyName.StartsWith("Unity.", StringComparison.OrdinalIgnoreCase) == false &&
                   assemblyName.StartsWith("UnityEngine.", StringComparison.OrdinalIgnoreCase) == false &&
                   assemblyName.StartsWith("UnityEditor.", StringComparison.OrdinalIgnoreCase) == false &&
                   assemblyName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) == false &&
                   assemblyName.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase) == false;
        }


        private static void ProcessAssemblyTypes(Assembly assembly, Type variableGenericType)
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }
            catch
            {
                return;
            }

            ProcessTypesUnsafe(types, variableGenericType);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ProcessTypesUnsafe(Type[] types, Type variableGenericType)
        {
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];

                if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                if (IsVariableType(type, variableGenericType) == false)
                {
                    continue;
                }

                Type genericArgument = GetGenericArgument(type);

                if (genericArgument == null)
                {
                    continue;
                }

                if (_GenericTypeToVariableType.ContainsKey(genericArgument) == false)
                {
                    _GenericTypeToVariableType[genericArgument] = type;
                    _AllVariableTypes.Add(type);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsVariableType(Type type, Type variableGenericType)
        {
            Type current = type;

            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType)
                {
                    Type genericTypeDef = current.GetGenericTypeDefinition();

                    if (ReferenceEquals(genericTypeDef, variableGenericType))
                    {
                        return true;
                    }
                }

                current = current.BaseType;
            }

            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Type GetGenericArgument(Type type)
        {
            Type current = type;
            Type variableGenericType = typeof(Variable<>);

            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType)
                {
                    Type genericTypeDef = current.GetGenericTypeDefinition();

                    if (ReferenceEquals(genericTypeDef, variableGenericType))
                    {
                        return current.GetGenericArguments()[0];
                    }
                }

                current = current.BaseType;
            }

            return null;
        }


        /// <summary>
        /// 특정 제네릭 타입 T에 대한 Variable&lt;T&gt; 구현 타입을 반환합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetVariableType<T>()
        {
            EnsureInitialized();
            _GenericTypeToVariableType.TryGetValue(typeof(T), out Type variableType);
            return variableType;
        }


        /// <summary>
        /// 모든 Variable&lt;T&gt; 구현 타입들을 반환합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type[] GetAllVariableTypes()
        {
            EnsureInitialized();
            return _cachedVariableTypesArray;
        }


        /// <summary>
        /// 특정 제네릭 타입에 대한 Variable&lt;T&gt; 구현 타입이 존재하는지 확인합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasVariableType<T>()
        {
            EnsureInitialized();
            return _GenericTypeToVariableType.ContainsKey(typeof(T));
        }


        /// <summary>
        /// 특정 제네릭 타입에 대한 Variable&lt;T&gt; 구현 타입이 존재하는지 확인합니다.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasVariableType(Type genericType)
        {
            return _GenericTypeToVariableType.ContainsKey(genericType);
        }


        /// <summary>
        /// 캐시를 강제로 새로고침합니다. (개발 중에만 사용)
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void RefreshCache()
        {
            _isInitialized = false;
            CollectTypes();
        }
        
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }
            
            CollectTypes();
        }
        
        
        
        #if UNITY_EDITOR
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
            if (TypeUtility.CanBeInstantiated(baseType))
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
            if (TypeUtility.CanBeInstantiated(resultType) == false)
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