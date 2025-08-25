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
            
            Debug.Assert(createdObject is NodeBase, $"{typeof(ObjectFactory)}: Failed to create node of type {nodeType}");
            
            NodeBase newNode = createdObject as NodeBase;
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


        public static BlackboardVariable CreateBBVariable(Type blackboardVariableType, string variableName, object defaultValue = null)
        {
            Debug.Assert(blackboardVariableType is not null, $"{typeof(ObjectFactory)}: Wrong blackboard variable type");
            BlackboardVariable createValue = (BlackboardVariable)Activator.CreateInstance(blackboardVariableType);
            Debug.Assert(createValue is not null, $"{typeof(ObjectFactory)}: Failed to create a blackboard variable.");
            
            createValue.key = variableName;
            createValue.type = blackboardVariableType;

            if (defaultValue != null)
            {
                createValue.boxedValue = defaultValue;
            }
            
            return createValue;
        }


        public static BlackboardVariable CreateSharedBBVariable(IBlackboard blackboard, UGUID guid, Type sharedBBVariableType)
        {
            Debug.Assert(sharedBBVariableType is not null, $"{typeof(ObjectFactory)}: Wrong blackboard variable type");
            
            BlackboardVariable bbVariable = (BlackboardVariable)Activator.CreateInstance(sharedBBVariableType);
            
            Debug.Assert(bbVariable is not null, $"{typeof(ObjectFactory)}: Failed to create a blackboard variable.");
            
            ISharedBlackboardVariable sharedVariable = bbVariable as ISharedBlackboardVariable;

            Debug.Assert(sharedVariable is not null, "bbVariable is not ISharedBlackboardVariable sharedVariable");
            sharedVariable.SetBlackboardAndVariableReference(blackboard, guid);

            BlackboardVariable foundVariable = blackboard.FindVariable(guid);
            bbVariable.key = blackboard.FindVariable(guid).key;
            bbVariable.boxedValue = foundVariable.boxedValue;
            bbVariable.type = typeof(BlackboardVariable<>).GetImplementedType(sharedBBVariableType.GenericTypeArguments[0]);
            bbVariable.isShared = true;
            return bbVariable;
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