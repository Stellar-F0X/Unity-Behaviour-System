using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TaskStreamer.Injection
{
    public class RuntimeInitAdapter : IVisitPropertyAdapter<GraphDictionary>,
                                      IVisitPropertyAdapter<Graph>,
                                      IVisitPropertyAdapter<NodeDictionary>,
                                      IVisitPropertyAdapter<NodeBase>,
                                      IVisitContravariantPropertyAdapter<BlackboardVariable>,
                                      IVisitPropertyAdapter<List<Transition>>, 
                                      IVisitPropertyAdapter<Transition>
    {
        public RuntimeInitAdapter(GraphVisitor dataContainer)
        {
            _dataContainer = dataContainer;

            _behaviorTreeBag = PropertyBag.GetPropertyBag<BehaviorTree>();
            _stateMachineBag = PropertyBag.GetPropertyBag<StateMachine>();
        }

        private readonly GraphVisitor _dataContainer;

        private readonly IPropertyBag<BehaviorTree> _behaviorTreeBag;
        private readonly IPropertyBag<StateMachine> _stateMachineBag;

        private NodeDictionary _newNodeDictionary;



        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            _newNodeDictionary = new NodeDictionary();

            foreach (KeyValuePair<UGUID, NodeBase> nodePairs in value)
            {
                NodeBase instantiated = Object.Instantiate(value[nodePairs.Key]);

                if (_dataContainer.debugMode)
                {
                    Debug.Log($"instantiated new node({value[nodePairs.Key].name}) is null? : {instantiated == null}");
                }
                else
                {
                    instantiated.name = instantiated.name.Replace("(Clone)", "");
                }

                _newNodeDictionary.Add(nodePairs.Key, instantiated);
                instantiated.InitializeOnInstantiated();
            }

            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)_newNodeDictionary;
            propertyBag.Accept(_dataContainer, ref dictionaryValue);

            value = _newNodeDictionary;
        }



        public void Visit<TContainer>(in VisitContext<TContainer, NodeBase> context, ref TContainer container, ref NodeBase value)
        {
            switch (_dataContainer.currentGraph.graphType)
            {
                case GraphType.BT: this.ProcessBehaviorTreeNode((BehaviorNodeBase)value); break;

                case GraphType.FSM: this.ProcessStateMachineNode((StateBase)value); break;

                case GraphType.GOAP: break; //TODO: 추후 GOAP를 추가한다면 여기도 추가로 Case를 추가해야 됨. 

                default: throw new ArgumentOutOfRangeException();
            }

            context.ContinueVisitation(ref container, ref value);
        }



        private void ProcessBehaviorTreeNode(BehaviorNodeBase instantiatedNode)
        {
            if (instantiatedNode == null || instantiatedNode.parent == null || instantiatedNode.parent.guid.IsEmpty())
            {
                if (instantiatedNode.nodeType != BehaviorNodeType.Root)
                {
                    // Warning: This behavior node does not have a parent node.
                    Debug.LogWarning($"{instantiatedNode.name} does not have a parent node.");
                }

                return;
            }

            BehaviorNodeBase parentNode = _newNodeDictionary[instantiatedNode.parent.guid] as BehaviorNodeBase;
            BehaviorNodeBase originalNode = _dataContainer.currentGraph.GetNodeByGuid(instantiatedNode.guid) as BehaviorNodeBase;

            //미리 만들어진 런타임용 부모 노드를 대상으로, 기존의 원본 자식 노드 대신, 새롭게 만들어진 런타임용 자식 노드를 대입. 
            parentNode.ChangeChild(originalNode, instantiatedNode);
        }



        private void ProcessStateMachineNode(StateBase stateNode) { }



        public void Visit<TContainer>(in VisitContext<TContainer, Graph> context, ref TContainer container, ref Graph value)
        {
            _dataContainer.currentGraph = value;

            if (_dataContainer.debugMode)
            {
                Debug.Log($"visit {value.name}({value.graphType}) graph.");
            }

            switch (value)
            {
                case BehaviorTree behaviorTree: _behaviorTreeBag.Accept(_dataContainer, ref behaviorTree); break;

                case StateMachine stateMachine: _stateMachineBag.Accept(_dataContainer, ref stateMachine); break;

                //TODO: GOAP

                default: Debug.LogError("Invalid graph type"); break;
            }

            _dataContainer.currentGraph = null;
        }



        public void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            IPropertyBag<Dictionary<UGUID, Graph>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, Graph>>();
            Dictionary<UGUID, Graph> dictionaryValue = value as Dictionary<UGUID, Graph>;
            propertyBag.Accept(_dataContainer, ref dictionaryValue);

            foreach (Graph graph in value.Values)
            {
                Debug.Assert(graph.entry != null, "entry node is null.");
                
                graph.InitializeOnEnterRuntime(_dataContainer.taskStreamer);
            }
        }



        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (value is null || value.variable is null || _dataContainer.blackboard == null || _dataContainer.blackboard.variables.Count == 0)
            {
                return;
            }

            if (value.isGlobal)
            {
                Variable foundVariable = _dataContainer.blackboard.FindVariable(value.name);

                // Error: The specified variable was not found in the blackboard.
                Debug.Assert(foundVariable != null, "Variable not found in blackboard.");

                BlackboardVariable blackboardVariable = value.Clone();
                blackboardVariable.variable = foundVariable;

                context.Property.SetValue(ref container, blackboardVariable);
            }
            else
            {
                BlackboardVariable blackboardVariable = value.Clone();

                context.Property.SetValue(ref container, blackboardVariable);
            }

            if (_dataContainer.debugMode)
            {
                Debug.Log($"{context.Property.Name} {value.name}({value.isGlobal})");
            }
        }
        
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
        {
            if (_dataContainer.debugMode)
            {
                Debug.Log($"{context.Property.Name}  Name: {value.name}  Des: {value.description}");
            }

            if (value.conditions.modules.Count == 0)
            {
                return;
            }

            IPropertyBag<List<ConditionModule>> bag = PropertyBag.GetPropertyBag<List<ConditionModule>>();
            List<ConditionModule> conditions = value.conditions.modules;
            bag.Accept(_dataContainer, ref conditions);
        }



        public void Visit<TContainer>(in VisitContext<TContainer, List<Transition>> context, ref TContainer container, ref List<Transition> value)
        {
            List<Transition> runtimeTransitions = new List<Transition>(value.Count);

            int transitionCount = value.Count;

            for (int i = 0; i < transitionCount; i++)
            {
                runtimeTransitions.Add(Object.Instantiate(value[i]));
            }

            value = runtimeTransitions;
            context.ContinueVisitation(ref container, ref value);
        }
    }
}