using System;
using NUnit.Framework;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.FSM;
using Unity.Properties;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaskStreamer.Runtime.Utility
{
    /// <summary> 'T'ask 'S'treamer Object Factory </summary>
    internal static class TSObjectFactory
    {
        public static BlackboardVariable CreateBlackboardVariable(Type genericBBVariableType, string name = "", object defaultValue = null)
        {
            Assert.IsTrue(genericBBVariableType != null, $"{typeof(TSObjectFactory)}: BlackboardVariableType is null");
            BlackboardVariable createdValue = BlackboardVariable.Create(genericBBVariableType, false);

            if (createdValue == null)
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: Failed to create BlackboardVariable of type {genericBBVariableType}");
                return null;
            }

            if (name.IsNotNullOrEmpty())
            {
                createdValue.key = name;
            }
            else
            {
                createdValue.key = BlackboardVariable.DEFAULT_VARIABLE_NAME;
            }
            
            createdValue.genericVariableType = genericBBVariableType;

            if (defaultValue is not null)
            {
                createdValue.boxedValue = defaultValue;
            }

            return createdValue;
        }



        /// <summary> 지정된 유형의 공유 BlackboardVariable 인스턴스를 생성한다. </summary>
        /// <param name="genericBBVariableType">생성할 BlackboardVariable의 Type.</param>
        /// <param name="reference">참조할 BlackboardAsset 객체.</param>
        /// <param name="variableGuid">BlackboardVariable의 고유 ID.</param>
        /// <returns>생성된 공유 BlackboardVariable 인스턴스. 실패 시 null 반환.</returns>
        public static BlackboardVariable CreateSharedBlackboardVariable(Type genericBBVariableType, BlackboardAsset reference, UGUID variableGuid)
        {
            Debug.Assert(reference != null, "blackboard is null");

            Debug.Assert(variableGuid.IsEmpty() == false, "variable guid is empty");

            Debug.Assert(genericBBVariableType != null, $"{typeof(TSObjectFactory)}: BlackboardVariableType is null");

            BlackboardVariable createdValue = BlackboardVariable.Create(genericBBVariableType, variableGuid, true);

            if (createdValue is not ISharedBlackboardVariable variable)
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: Failed to create BlackboardVariable of type {genericBBVariableType}");
                return null;
            }

            createdValue.genericVariableType = genericBBVariableType;
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
                Debug.LogError($"{typeof(TSObjectFactory)}: NodeType is not NodeBase");
                return null;
            }

            object createdObject = Activator.CreateInstance(nodeType);

            if (createdObject is not NodeBase newNode)
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: Failed to create node of type {nodeType}");
                return null;
            }

            newNode.position = position;
            newNode.guid = UGUID.Create();
            newNode.name = StringUtility.ToNicifyName(nodeType.Name);

            IPropertyBag propertyBag = PropertyBag.GetPropertyBag(nodeType);
            propertyBag.Accept(new BBVariableFieldInitializer(), ref createdObject);

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
                Debug.LogError($"{typeof(TSObjectFactory)}: State's From guid is empty.");
                return null;
            }

            if (to.guid.IsEmpty())
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: State's To guid is empty.");
                return null;
            }

            if (from.TryGetTransition(to.guid, out Transition foundTransition))
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: Transition already exists.");
                return foundTransition;
            }

            
            Type transitionType = null;
            Transition resultTransition = null;

            if (from is AnyState)
            {
                transitionType = typeof(AnyTransition);
                resultTransition = new AnyTransition(from, to);
            }
            else
            {
                transitionType = typeof(Transition);
                resultTransition = new Transition(from, to);
            }

            object reference = resultTransition;
            IPropertyBag propertyBag = PropertyBag.GetPropertyBag(transitionType);
            propertyBag.Accept(new BBVariableFieldInitializer(), ref reference);
            return resultTransition;
        }



        /// <summary> 지정된 GraphAsset을 기반으로 FSM 또는 BT 그래프를 생성합니다. </summary>
        /// <param name="asset">새로운 그래프를 생성하는 데 사용할 GraphAsset입니다.</param>
        /// <param name="graphType">생성할 그래프의 유형(FSM 또는 BT)입니다.</param>
        /// <param name="graph">생성된 그래프 객체의 참조입니다.</param>
        /// <param name="graphName">생성할 그래프의 이름입니다.</param>
        public static Graph CreateGraph(GraphAsset asset, GraphType graphType, string graphName)
        {
            Graph graph = null;

            Debug.Assert(asset != null, $"{typeof(TSObjectFactory)}: GraphAsset is null");

            switch (graphType)
            {
                case GraphType.FSM: graph = StateMachine.CreateGraph(graphName, asset); break;

                case GraphType.BT: graph = BehaviorTree.CreateGraph(graphName, asset); break;
            }

            Debug.Assert(graph != null, $"{typeof(TSObjectFactory)}: Failed to create a graph.");
            return graph;
        }




        /// <summary> 새로운 ServiceBase 객체를 생성한다. </summary>
        /// <param name="serviceType">생성할 ServiceBase의 타입</param>
        /// <returns>생성된 ServiceBase 객체 또는 null</returns>
        public static ServiceBase CreateService(Type serviceType)
        {
            if (serviceType == null || typeof(ServiceBase).IsAssignableFrom(serviceType) == false)
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: Type {serviceType} is not derived from ServiceBase");
                return null;
            }

            object createdObject = Activator.CreateInstance(serviceType);
            ServiceBase result = createdObject as ServiceBase;

            if (result == null)
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: Failed to create service of type {serviceType}");
                return null;
            }

            IPropertyBag propertyBag = PropertyBag.GetPropertyBag(serviceType);
            propertyBag.Accept(new BBVariableFieldInitializer(), ref createdObject);
            return result;
        }




        /// <summary> 지정된 유형의 Condition 모듈을 생성한다. </summary>
        /// <param name="conditionType">생성할 Condition의 Type.</param>
        /// <returns>생성된 Condition 인스턴스. 실패 시 null 반환.</returns>
        public static Condition CreateConditionModule(Type conditionType)
        {
            Debug.Assert(conditionType is not null, $"{typeof(TSObjectFactory)}: Wrong condition type");

            object createdObject = Activator.CreateInstance(conditionType);
            Condition result = createdObject as Condition;
            
            // 인스턴스 생성 및 타입 검사
            if (result == null)
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: Failed to create a condition module of type '{conditionType?.FullName}'.");
                return null;
            }
            
            IPropertyBag propertyBag = PropertyBag.GetPropertyBag(conditionType);
            propertyBag.Accept(new BBVariableFieldInitializer(), ref createdObject);
            return result;
        }



        /// <summary> 새로운 BlackboardAsset 객체를 생성한다. </summary>
        /// <param name="blackboardName">생성할 BlackboardAsset의 이름</param>
        /// <returns>생성된 BlackboardAsset 객체</returns>
        public static BlackboardAsset CreateBlackboardAssetInstance(string blackboardName)
        {
            BlackboardAsset blackboardAsset = ScriptableObject.CreateInstance<BlackboardAsset>();
            blackboardAsset.name = blackboardName;
            UnityEditor.EditorUtility.SetDirty(blackboardAsset);
            return blackboardAsset;
        }



        /// <summary> 주어진 BlackboardAsset을 복제한다. </summary>
        /// <param name="asset">복제할 대상 BlackboardAsset</param>
        /// <returns>복제된 새로운 BlackboardAsset 객체</returns>
        public static BlackboardAsset CloneBlackboardAssetInstance(this BlackboardAsset asset)
        {
            BlackboardAsset clone = Object.Instantiate(asset);
            clone.name = clone.name.Replace("(Clone)", "");
            return clone;
        }



        public static GraphAsset CreateGraphAssetInstance(string graphAssetName, GraphType mainGraphType, string mainGraphName, bool withBlackboard = true)
        {
            GraphAsset graphAsset = ScriptableObject.CreateInstance<GraphAsset>();
            graphAsset.name = graphAssetName;
            graphAsset.mainGraphType = mainGraphType;
            graphAsset.graphGuid = UGUID.Create();
            graphAsset.main = TSObjectFactory.CreateGraph(graphAsset, mainGraphType, mainGraphName);

            if (withBlackboard)
            {
                graphAsset.blackboard = TSObjectFactory.CreateBlackboardAssetInstance("Blackboard");
            }
            
            UnityEditor.EditorUtility.SetDirty(graphAsset);
            return graphAsset;
        }
        
        
        
        public static GraphAsset CreateGraphAssetFile(string graphAssetName, GraphType mainGraphType, string mainGraphName, bool withBlackboard = true)
        {
            GraphAsset graphAsset = CreateGraphAssetInstance(graphAssetName, mainGraphType, mainGraphName, withBlackboard);
            UnityEditor.ProjectWindowUtil.CreateAsset(graphAsset, $"New {graphAssetName} Graph.asset");
            return graphAsset;
        }



        public static GraphAsset TestCreateGraphAssetFile(string path, string graphAssetName, GraphType mainGraphType, string mainGraphName, bool withBlackboard = true)
        {
            GraphAsset graphAsset = CreateGraphAssetInstance(graphAssetName, mainGraphType, mainGraphName, withBlackboard);
            UnityEditor.AssetDatabase.CreateAsset(graphAsset, path);
            UnityEditor.AssetDatabase.SaveAssets();
            return graphAsset;
        }
#endif
    }
}