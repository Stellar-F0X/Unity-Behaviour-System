using System;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Injection;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Utility
{
    /// <summary> 'T'ask 'S'treamer Object Factory </summary>
    internal static class ObjectFactory
    {
         /// <summary> 새로운 BlackboardVariable 객체를 생성한다. </summary>
        /// <param name="implementedType">생성할 BlackboardVariable의 타입</param>
        /// <param name="name">BlackboardVariable의 이름 (기본값: 빈 문자열)</param>
        /// <param name="defaultValue">BlackboardVariable의 기본 값 (기본값: null)</param>
        /// <returns>생성된 BlackboardVariable 객체 또는 null</returns>
        public static BlackboardVariable CreateBlackboardVariable(Type implementedType, string name = "", object defaultValue = null)
        {
            Debug.Assert(implementedType != null, $"{typeof(ObjectFactory)}: BlackboardVariableType is null");

            BlackboardVariable createdValue = BlackboardVariable.Create(implementedType, false);

            if (createdValue == null)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Failed to create BlackboardVariable of type {implementedType}");
                return null;
            }

            createdValue.key = name.IsNotNullOrEmpty() ? name : BlackboardVariable.DEFAULT_VARIABLE_NAME;
            createdValue.implementedType = implementedType;

            if (defaultValue is not null)
            {
                createdValue.boxedValue = defaultValue;
            }
            
            return createdValue;
        }



        /// <summary> 지정된 유형의 공유 BlackboardVariable 인스턴스를 생성한다. </summary>
        /// <param name="implementedType">생성할 BlackboardVariable의 Type.</param>
        /// <param name="reference">참조할 BlackboardAsset 객체.</param>
        /// <param name="variableGuid">BlackboardVariable의 고유 ID.</param>
        /// <returns>생성된 공유 BlackboardVariable 인스턴스. 실패 시 null 반환.</returns>
        public static BlackboardVariable CreateSharedBlackboardVariable(Type implementedType, BlackboardAsset reference, UGUID variableGuid)
        {
            Debug.Assert(reference != null, "blackboard is null");

            Debug.Assert(variableGuid.IsEmpty() == false, "variable guid is empty");
            
            Debug.Assert(implementedType != null, $"{typeof(ObjectFactory)}: BlackboardVariableType is null");

            BlackboardVariable createdValue = BlackboardVariable.Create(implementedType, variableGuid, true);

            if (createdValue is not ISharedBlackboardVariable variable)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Failed to create BlackboardVariable of type {implementedType}");
                return null;
            }
            
            createdValue.implementedType = implementedType;
            
            variable.SetBlackboardReference(reference);
            return createdValue;
        }
        
        
        
#if UNITY_EDITOR
        /// <summary> 지정된 유형의 노드를 생성한다. </summary>
        /// <param name="nodeType">생성할 노드의 Type.</param>
        /// <param name="position">생성된 노드의 초기 위치 (기본값: (0, 0)).</param>
        /// <returns>생성된 NodeBase 인스턴스. 실패 시 null 반환.</returns>
        public static NodeBase CreateNode(Type nodeType, Vector2Int position = default)
        {
            if (typeof(NodeBase).IsAssignableFrom(nodeType) == false)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: NodeType is not NodeBase");
                return null;
            }

            if (PropertyBag.Exists(nodeType) == false)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: The {nodeType} hasn't {nameof(GeneratePropertyBagAttribute)}");
                return null;
            }

            object createdObject = Activator.CreateInstance(nodeType);

            if (createdObject is not NodeBase newNode)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Failed to create node of type {nodeType}");
                return null;
            }

            newNode.position = position;
            newNode.guid = UGUID.Create();
            newNode.name = StringUtility.ToNicifyName(nodeType.Name);

            IPropertyBag propertyBag = PropertyBag.GetPropertyBag(nodeType);
            propertyBag.Accept(new BlackboardVariableFieldInitializeVisitor(), ref createdObject);

            newNode.OnCreateInEditor();
            return newNode;
        }



        /// <summary> 두 상태 간의 전이를 생성합니다. </summary>
        /// <param name="from"> 전이의 시작 상태 </param>
        /// <param name="to"> 전이의 도착 상태 </param>
        /// <return> 생성된 Transition 객체를 반환하며, 오류 발생 시 null을 반환합니다. </return>
        public static Transition CreateTransition(StateBase from, StateBase to)
        {
            if (from.guid.IsEmpty())
            {
                Debug.LogError($"{typeof(ObjectFactory)}: State's From guid is empty.");
                return null;
            }

            if (to.guid.IsEmpty())
            {
                Debug.LogError($"{typeof(ObjectFactory)}: State's To guid is empty.");
                return null;
            }

            if (from.TryGetTransition(to.guid, out Transition foundTransition))
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Transition already exists.");
                return foundTransition;
            }

            return new Transition(from, to);
        }



        /// <summary> 지정된 GraphAsset을 기반으로 FSM 또는 BT 그래프를 생성합니다. </summary>
        /// <param name="asset">새로운 그래프를 생성하는 데 사용할 GraphAsset입니다.</param>
        /// <param name="graphType">생성할 그래프의 유형(FSM 또는 BT)입니다.</param>
        /// <param name="graph">생성된 그래프 객체의 참조입니다.</param>
        /// <param name="graphName">생성할 그래프의 이름입니다.</param>
        public static void CreateGraph(GraphAsset asset, GraphType graphType, ref Graph graph, string graphName)
        {
            if (asset == null)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: GraphAsset is null");
                return;
            }

            switch (graphType)
            {
                case GraphType.FSM: graph = StateMachine.CreateGraph(graphName, asset); break;

                case GraphType.BT: graph = BehaviorTree.CreateGraph(graphName, asset); break;
            }

            Debug.Assert(graph != null, $"{typeof(ObjectFactory)}: Failed to create a graph.");
        }



        /// <summary> 지정된 유형의 Condition 모듈을 생성한다. </summary>
        /// <param name="conditionType">생성할 Condition의 Type.</param>
        /// <returns>생성된 Condition 인스턴스. 실패 시 null 반환.</returns>
        public static Condition CreateConditionModule(Type conditionType)
        {
            Debug.Assert(conditionType is not null, $"{typeof(ObjectFactory)}: Wrong condition type");

            // 인스턴스 생성 및 타입 검사
            if (Activator.CreateInstance(conditionType) is not Condition module)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Failed to create a condition module of type '{conditionType?.FullName}'.");
                return null;
            }

            // 베이스 타입 및 제네릭 인자 추출
            Type baseType = conditionType.BaseType;

            if (baseType == null || baseType.IsGenericType == false)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Condition type '{conditionType.FullName}' does not have a generic base type to infer element type.");
                return module;
            }

            Type[] genericArgs = baseType.GenericTypeArguments;
            if (genericArgs == null || genericArgs.Length == 0 || genericArgs[0] == null)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Could not determine generic argument for '{conditionType.FullName}'.");
                return module;
            }

            Type elementType = genericArgs[0];

            // BlackboardVariable<> 타입 구성
            Type bbType = typeof(BlackboardVariable<>).GetImplementedType(elementType);

            if (bbType == null)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Failed to construct BlackboardVariable<> for element type '{elementType.FullName}'.");
                return module;
            }

            // 캡슐 변수 생성
            module.lVariable = ObjectFactory.CreateBlackboardVariable(bbType);
            module.rVariable = ObjectFactory.CreateBlackboardVariable(bbType);

            module.lVariable.usage = VariableUsage.Condition;
            module.rVariable.usage = VariableUsage.Condition;

            // ComparableAttribute로 비교 타입 구성 (기본값 사용)
            ComparableAttribute comparable = conditionType.GetAttribute<ComparableAttribute>();
            module.configuredComparisonType = comparable?.comparison ?? Condition.DEFAULT_COMPARISON;

            return module;
        }
#endif
    }
}