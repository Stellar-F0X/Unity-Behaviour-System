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
            DefaultVisitProcessor visitWorker = new DefaultVisitProcessor();
            visitWorker.AddAdapter(new VariableFieldAllocateProcess());

            propertyBag.Accept(visitWorker, ref createdObject);

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


        /// <summary> 새로운 BlackboardVariable 객체를 생성한다. </summary>
        /// <param name="blackboardVariableType">생성할 BlackboardVariable의 타입</param>
        /// <param name="name">BlackboardVariable의 이름 (기본값: 빈 문자열)</param>
        /// <param name="defaultValue">BlackboardVariable의 기본 값 (기본값: null)</param>
        /// <returns>생성된 BlackboardVariable 객체 또는 null</returns>
        public static BlackboardVariable CreateBlackboardVariable(Type blackboardVariableType, string name = "", object defaultValue = null)
        {
            Debug.Assert(blackboardVariableType != null, $"{typeof(ObjectFactory)}: BlackboardVariableType is null");

            BlackboardVariable createdValue = BlackboardVariable.Create(blackboardVariableType, false);
            
            if (createdValue == null)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Failed to create BlackboardVariable of type {blackboardVariableType}");
                return null;
            }

            createdValue.key = name.IsNotNullOrEmpty() ? name : BlackboardVariable.DEFAULT_VARIABLE_NAME;
            createdValue.implementedType = blackboardVariableType;
            createdValue.boxedValue = defaultValue;
            return createdValue;
        }


        /// <summary> Creates a shared BlackboardVariable instance of the specified type. </summary>
        /// <param name="blackboardVariableType">The type of the BlackboardVariable to create.</param>
        /// <returns>Returns the created shared BlackboardVariable or null if creation failed.</returns>
        public static BlackboardVariable CreateSharedBlackboardVariable(Type blackboardVariableType)
        {
            Debug.Assert(blackboardVariableType != null, $"{typeof(ObjectFactory)}: BlackboardVariableType is null");

            BlackboardVariable createdValue = BlackboardVariable.Create(blackboardVariableType, true);
            
            if (createdValue == null)
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Failed to create BlackboardVariable of type {blackboardVariableType}");
                return null;
            }

            createdValue.implementedType = blackboardVariableType;
            return createdValue;
        }


        /// <summary> 지정된 GraphAsset을 기반으로 FSM 또는 BT 그래프를 생성합니다. </summary>
        /// <param name="asset">새로운 그래프를 생성하는 데 사용할 GraphAsset입니다.</param>
        /// <param name="graphType">생성할 그래프의 유형(FSM 또는 BT)입니다.</param>
        /// <param name="graph">생성된 그래프 객체의 참조입니다.</param>
        /// <param name="graphName">생성할 그래프의 이름입니다.</param>
        public static void CreateGraph(GraphAsset asset, GraphType graphType, ref Graph graph, string graphName)
        {
            Debug.Assert(asset != null, $"{typeof(ObjectFactory)}: GraphAsset is null");

            switch (graphType)
            {
                case GraphType.FSM: graph = StateMachine.CreateGraph(graphName, asset); break;

                case GraphType.BT: graph = BehaviorTree.CreateGraph(graphName, asset); break;
            }

            Debug.Assert(graph != null, $"{typeof(ObjectFactory)}: Failed to create a graph.");
        }
    }
#endif
}