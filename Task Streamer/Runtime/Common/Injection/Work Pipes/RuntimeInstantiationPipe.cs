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
    internal class RuntimeInstantiationPipe : GraphWorkPipeBase,
                                              IVisitPropertyAdapter<NodeDictionary>,
                                              IVisitPropertyAdapter<KeyValuePair<UGUID, NodeBase>>,
                                              IVisitPropertyAdapter<List<Transition>>,
                                              IVisitPropertyAdapter<Transition>,
                                              IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public RuntimeInstantiationPipe(GraphWorker worker) : base(worker) { }


        private NodeDictionary _newNodeDictionary;



        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            _newNodeDictionary = new NodeDictionary();

            foreach (KeyValuePair<UGUID, NodeBase> nodePairs in value)
            {
                NodeBase instantiation = Object.Instantiate(value[nodePairs.Key]);
                instantiation.name = instantiation.name.Replace("(Clone)", "");

                _newNodeDictionary.Add(nodePairs.Key, instantiation);
                instantiation.InitializeOnInstantiated();
            }

            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)_newNodeDictionary;
            propertyBag.Accept(_worker, ref dictionaryValue);
            value = _newNodeDictionary;
        }



        public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, NodeBase>> context, ref TContainer container, ref KeyValuePair<UGUID, NodeBase> pair)
        {
            switch (_worker.currentGraph.graphType)
            {
                case GraphType.BT: this.ProcessBehaviorTreeNode((BehaviorNodeBase)pair.Value); break;

                case GraphType.FSM: break;

                case GraphType.GOAP: break; //TODO: 추후 GOAP를 추가한다면 여기도 추가로 Case를 추가해야 됨. 

                default: throw new ArgumentOutOfRangeException();
            }

            //굳이 이렇게 하는 이유는 PropertyVisit이 Key에 방문하는 것을 무시를 위해서.
            IPropertyBag bag = PropertyBag.GetPropertyBag(pair.Value.GetType());
            Debug.Assert(bag != null, $"Property bag not found for {pair.Value.name}");

            object reference = pair.Value; //어차피 노드는 항상 Class 타입이므로, object로 형변환해도 Boxing/Unboxing은 문제 없음.
            bag.Accept(_worker, ref reference);
        }



        private void ProcessBehaviorTreeNode(BehaviorNodeBase instantiatedNode)
        {
            if (instantiatedNode == null || instantiatedNode.parent == null || instantiatedNode.parent.guid.IsEmpty())
            {
                // Warning: This behavior node does not have a parent node.
                Debug.Assert(instantiatedNode.nodeType == BehaviorNodeType.Root, $"{instantiatedNode.name} does not have a parent node.");
                return;
            }

            BehaviorNodeBase parentNode = _newNodeDictionary[instantiatedNode.parent.guid] as BehaviorNodeBase;
            Debug.Assert(parentNode != null, "Parent node is null.");

            BehaviorNodeBase originalNode = _worker.currentGraph.GetNodeByGuid(instantiatedNode.guid) as BehaviorNodeBase;
            Debug.Assert(originalNode != null, "Original node is null.");

            //미리 만들어진 런타임용 부모 노드를 대상으로, 기존의 원본 자식 노드 대신, 새롭게 만들어진 런타임용 자식 노드를 대입.
            parentNode?.ChangeChild(originalNode, instantiatedNode);
        }



        public override void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            base.Visit(context, ref container, ref value);

            foreach (Graph graph in value.Values)
            {
                Debug.Assert(graph.entry != null, "entry node is null.");
                graph.InitializeOnEnterRuntime(_worker.taskStreamer);
            }
        }



        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (value?.variable is null || _worker.blackboard == null || _worker.blackboard.count == 0)
            {
                return;
            }

            if (value.isGlobal == false)
            {
                context.Property.SetValue(ref container, value.Clone());
                return;
            }

            Variable foundVariable = _worker.blackboard.FindVariable(value.guid);
            Debug.Assert(foundVariable != null, "Variable not found in blackboard.");

            BlackboardVariable blackboardVariable = value.Clone(); 
            blackboardVariable.variable = foundVariable;
            context.Property.SetValue(ref container, blackboardVariable);
        }



        public void Visit<TContainer>(in VisitContext<TContainer, List<Transition>> context, ref TContainer container, ref List<Transition> value)
        {
            List<Transition> runtimeTransitions = new List<Transition>(value.Count);

            int transitionCount = value.Count;

            for (int i = 0; i < transitionCount; i++)
            {
                Transition instantiation = Object.Instantiate(value[i]);
                runtimeTransitions.Add(instantiation);
            }

            value = runtimeTransitions;
            context.ContinueVisitation(ref container, ref value);
        }



        public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
        {
            value.sourceNode = _newNodeDictionary[value.fromNodeGuid];
            value.destinationNode = _newNodeDictionary[value.toNodeGuid];
            
            if (value.conditions.modules.Count == 0) //ConditionModule이 없다면 BBVariable을 할당하지 않아도 되므로 Early Return.
            {
                return;
            }

            IPropertyBag<List<ConditionModule>> bag = PropertyBag.GetPropertyBag<List<ConditionModule>>();
            List<ConditionModule> conditions = value.conditions.modules;
            bag.Accept(_worker, ref conditions);
        }
    }
}