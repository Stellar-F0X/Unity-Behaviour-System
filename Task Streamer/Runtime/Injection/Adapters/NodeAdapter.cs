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
    public class NodeAdapter : IVisitPropertyAdapter<NodeDictionary>, IVisitPropertyAdapter<NodeBase>
    {
        public NodeAdapter(GraphVisitor visitor)
        {
            _visitor = visitor;
        }

        private readonly GraphVisitor _visitor;

        private NodeDictionary _newNodeDictionary;


        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            _newNodeDictionary = new NodeDictionary();

            foreach (KeyValuePair<UGUID, NodeBase> nodePairs in value)
            {
                NodeBase instantiated = Object.Instantiate(value[nodePairs.Key]);

                if (_visitor.debug)
                {
                    Debug.Log($"instantiated new node({value[nodePairs.Key].name}) is null? : {instantiated == null}");
                }
                
                //instantiated.name = instantiated.name.Replace("(Clone)", ""); //TODO: 변경해야 됨 
                _newNodeDictionary.Add(nodePairs.Key, instantiated);
                instantiated.InitializeOnInstantiated();
            }

            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)_newNodeDictionary;
            propertyBag.Accept(_visitor, ref dictionaryValue);

            value = _newNodeDictionary;
        }


        public void Visit<TContainer>(in VisitContext<TContainer, NodeBase> context, ref TContainer container, ref NodeBase value)
        {
            switch (_visitor.currentGraph.graphType)
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
            BehaviorNodeBase originalNode = _visitor.currentGraph.GetNodeByGuid(instantiatedNode.guid) as BehaviorNodeBase;

            //미리 만들어진 런타임용 부모 노드를 대상으로, 기존의 원본 자식 노드 대신, 새롭게 만들어진 런타임용 자식 노드를 대입. 
            parentNode.ChangeChild(originalNode, instantiatedNode);
        }


        private void ProcessStateMachineNode(StateBase stateNode) { }
    }
}