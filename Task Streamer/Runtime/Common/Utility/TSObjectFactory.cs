using System;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Injection;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Utility
{
    /// <summary> 'T'ask 'S'treamer Object Factory </summary>
    public static class TSObjectFactory
    {
#if UNITY_EDITOR
        public static NodeBase CreateNode(Type nodeType, Vector2Int position = default)
        {
            if (typeof(NodeBase).IsAssignableFrom(nodeType) == false)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: NodeType is not NodeBase");
            }

            object createdObject = Activator.CreateInstance(nodeType);

            if (createdObject is not NodeBase newNode)
            {
                throw new Exception($"{typeof(TSObjectFactory)}: Failed to create node of type {nodeType}");
            }

            newNode.guid = UGUID.Create();
            newNode.name = StringUtility.ApplySpacing(nodeType.Name);
            newNode.position = position;

            IPropertyBag bag = PropertyBag.GetPropertyBag(nodeType);
            DefaultVisitProcessor visitWorker = new DefaultVisitProcessor();
            visitWorker.AddAdapter(new VariableFieldAllocateProcess());
            bag.Accept(visitWorker, ref createdObject);
            newNode.OnCreateInEditor();
            return newNode;
        }


        public static Transition CreateTransition(StateBase from, StateBase to)
        {
            if (from.TryGetTransition(to.guid, out _))
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: Transition already exists.");
            }

            Transition newTransition = new Transition(from, to);
            newTransition.name = $"{from.guid}.{to.guid}";
            return newTransition;
        }


        public static Variable CreateVariable(Type variableType, object defaultValue = null, bool isLocal = false)
        {
            Debug.Assert(variableType is not null, "Failed to create a variable.");
            Variable newVariable = Activator.CreateInstance(variableType) as Variable;
            Debug.Assert(newVariable is not null, "Failed to create a variable.");

            newVariable.key = isLocal ? "#Constant Variable#" : $"New {variableType.Name}";
            newVariable.type = variableType;

            if (defaultValue != null)
            {
                newVariable.boxedValue = defaultValue;
            }
            
            return newVariable;
        }



        public static BlackboardVariable CreateBlackboardVariable(Type blackboardVariableType, object defaultValue = null, bool isLocal = false)
        {
            BlackboardVariable createValue = (BlackboardVariable)Activator.CreateInstance(blackboardVariableType);
            
            Type variableType = typeof(Variable<>).GetImplementedType(blackboardVariableType.GenericTypeArguments[0]);
            createValue.variable = TSObjectFactory.CreateVariable(variableType, defaultValue, isLocal);
            createValue.isGlobal = !isLocal;
            return createValue;
        }
        

        public static Condition CreateConditionModule(Type conditionType)
        {
            Condition module = Activator.CreateInstance(conditionType) as Condition;
            Debug.Assert(module is not null, "Failed to create a condition module.");

            Type bbType = typeof(BlackboardVariable<>).MakeGenericType(conditionType!.BaseType!.GenericTypeArguments[0]);
            module.encapsulatedLeftVariable = CreateBlackboardVariable(bbType, isLocal: true);
            module.encapsulatedRightVariable = CreateBlackboardVariable(bbType, isLocal: true);
            
            ComparableAttribute comparable = conditionType.GetAttribute<ComparableAttribute>();

            Debug.Assert(comparable != null, "comparable is null");
            
            module.configuredComparisonType = comparable.comparison;
            return module;
        }


        public static void CreateGraph(GraphAsset asset, GraphType graphType, ref Graph graph, string graphName)
        {
            if (asset == null)
            {
                Debug.LogError($"{typeof(TSObjectFactory)}: GraphAsset is null.");
                return;
            }

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