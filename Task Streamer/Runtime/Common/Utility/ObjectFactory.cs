using System;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Injection;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Utility
{
    /// <summary> 'T'ask 'S'treamer Object Factory </summary>
    public static class ObjectFactory
    {
#if UNITY_EDITOR
        public static NodeBase CreateNode(Type nodeType, Vector2Int position = default)
        {
            if (typeof(NodeBase).IsAssignableFrom(nodeType) == false)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: NodeType is not NodeBase");
            }

            if (PropertyBag.Exists(nodeType) == false)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: The nodeType({nodeType}) hasn't {nameof(GeneratePropertyBagAttribute)}");
            }

            object createdObject = Activator.CreateInstance(nodeType);

            if (createdObject is not NodeBase newNode)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: Failed to create node of type {nodeType}");
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


        public static Transition CreateTransition(StateBase from, StateBase to)
        {
            if (from.TryGetTransition(to.guid, out Transition foundTransition))
            {
                Debug.LogError($"{typeof(ObjectFactory)}: Transition already exists.");
                return foundTransition;
            }

            return new Transition(from, to);
        }


        public static BlackboardVariable CreateBBVariable(Type blackboardVariableType, string variableName, object defaultValue = null, bool isLocal = false)
        {
            if (blackboardVariableType is null)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: Wrong blackboard variable type");
            }

            BlackboardVariable createValue = (BlackboardVariable)Activator.CreateInstance(blackboardVariableType);

            if (createValue is null)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: Failed to create a blackboard variable.");
            }
            
            createValue.isGlobal = !isLocal;
            createValue.key = variableName;
            createValue.type = blackboardVariableType;

            if (defaultValue != null)
            {
                createValue.boxedValue = defaultValue;
            }
            
            return createValue;
        }


        public static Condition CreateConditionModule(Type conditionType)
        {
            if (conditionType is null)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: Wrong condition type");
            }

            Condition module = Activator.CreateInstance(conditionType) as Condition;

            if (module is null)
            {
                throw new ArgumentException($"{typeof(ObjectFactory)}: Failed to create a condition module.");
            }

            Type bbType = typeof(BlackboardVariable<>).MakeGenericType(conditionType!.BaseType!.GenericTypeArguments[0]);
            module.encapsulatedLeftVariable = CreateBBVariable(bbType, "", true);
            module.encapsulatedRightVariable = CreateBBVariable(bbType, "", true);

            ComparableAttribute comparable = conditionType.GetAttribute<ComparableAttribute>();
            module.configuredComparisonType = comparable is null ? Condition.DEFAULT_COMPARISON : comparable.comparison;
            return module;
        }


        public static void CreateGraph(GraphAsset asset, GraphType graphType, ref Graph graph, string graphName)
        {
            Debug.Assert(asset != null, "GraphAsset is null");

            switch (graphType)
            {
                case GraphType.FSM: graph = StateMachine.CreateGraph(graphName, asset); break;

                case GraphType.BT: graph = BehaviorTree.CreateGraph(graphName, asset); break;
            }

            Debug.Assert(graph != null, "Failed to create a graph.");
        }
    }
#endif
}