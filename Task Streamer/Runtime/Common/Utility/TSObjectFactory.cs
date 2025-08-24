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

            if (PropertyBag.Exists(nodeType) == false)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: The nodeType({nodeType}) hasn't {nameof(GeneratePropertyBagAttribute)}");
            }

            object createdObject = Activator.CreateInstance(nodeType);

            if (createdObject is not NodeBase newNode)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: Failed to create node of type {nodeType}");
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
                Debug.LogError($"{typeof(TSObjectFactory)}: Transition already exists.");
                return foundTransition;
            }

            return new Transition(from, to);
        }


        public static Variable CreateVariable(Type variableType, string variableName, object defaultValue = null, bool isLocal = false)
        {
            if (variableType is null)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: Wrong variable type");
            }

            Variable newVariable = Activator.CreateInstance(variableType) as Variable;

            if (newVariable is null)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: Failed to create a variable.");
            }

            newVariable.key = variableName;
            newVariable.type = variableType;

            if (defaultValue != null)
            {
                newVariable.boxedValue = defaultValue;
            }

            return newVariable;
        }


        public static BlackboardVariable CreateBlackboardVariable(Type blackboardVariableType, string variableName, object defaultValue = null, bool isLocal = false)
        {
            if (blackboardVariableType is null)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: Wrong blackboard variable type");
            }

            BlackboardVariable createValue = (BlackboardVariable)Activator.CreateInstance(blackboardVariableType);

            if (createValue is null)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: Failed to create a blackboard variable.");
            }

            Type variableType = typeof(Variable<>).GetImplementedType(blackboardVariableType.GenericTypeArguments[0]);

            if (variableType is null)
            {
                return null;
            }

            createValue.variable = TSObjectFactory.CreateVariable(variableType, variableName, defaultValue, isLocal);
            createValue.isGlobal = !isLocal;
            return createValue;
        }


        public static Condition CreateConditionModule(Type conditionType)
        {
            if (conditionType is null)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: Wrong condition type");
            }

            Condition module = Activator.CreateInstance(conditionType) as Condition;

            if (module is null)
            {
                throw new ArgumentException($"{typeof(TSObjectFactory)}: Failed to create a condition module.");
            }

            Type bbType = typeof(BlackboardVariable<>).MakeGenericType(conditionType!.BaseType!.GenericTypeArguments[0]);
            module.encapsulatedLeftVariable = CreateBlackboardVariable(bbType, Variable.DEFAULT_LOCAL_VARIABLE_NAME, true);
            module.encapsulatedRightVariable = CreateBlackboardVariable(bbType, Variable.DEFAULT_LOCAL_VARIABLE_NAME, true);

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