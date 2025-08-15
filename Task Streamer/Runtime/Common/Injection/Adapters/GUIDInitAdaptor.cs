using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    public class GUIDInitAdaptor : IVisitPropertyAdapter<GraphDictionary>,
                                   IVisitPropertyAdapter<Graph>,
                                   IVisitPropertyAdapter<NodeDictionary>
    {
        public GUIDInitAdaptor(GraphVisitor dataContainer)
        {
            _dataContainer = dataContainer;

            _behaviorTreeBag = PropertyBag.GetPropertyBag<BehaviorTree>();
            _stateMachineBag = PropertyBag.GetPropertyBag<StateMachine>();
        }

        private readonly GraphVisitor _dataContainer;

        private readonly IPropertyBag<BehaviorTree> _behaviorTreeBag;
        private readonly IPropertyBag<StateMachine> _stateMachineBag;
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            GraphDictionary newDictionary = new GraphDictionary();

            foreach (KeyValuePair<UGUID, Graph> graphPair in value)
            {
                UGUID newGuid = UGUID.Create();
                UGUID baseGuid = graphPair.Value.baseGraphGuid;
                
                graphPair.Value.guid = newGuid;

                if (baseGuid.IsEmpty() == false) 
                {
                    graphPair.Value.baseGraphGuid = value[baseGuid].guid; 
                }
                
                newDictionary.Add(newGuid, graphPair.Value); 
            }
            
            IPropertyBag<Dictionary<UGUID, Graph>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, Graph>>();
            Dictionary<UGUID, Graph> dictionaryValue = newDictionary as Dictionary<UGUID, Graph>;
            propertyBag.Accept(_dataContainer, ref dictionaryValue);
            value = newDictionary;
        }
        
        
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

                default: Debug.LogError("Invalid graph type"); break;
            }
            
            _dataContainer.currentGraph = null;
        }
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            NodeDictionary newNodeDictionary = new NodeDictionary();
            
            foreach (KeyValuePair<UGUID, NodeBase> pair in value)
            {
                NodeBase node = value[pair.Key]; 
                node.guid = UGUID.Create(); 
                newNodeDictionary.Add(node.guid, node);

                if (node is ISubGraph subGraphNode)
                {
                    UGUID guid = subGraphNode.subGraphGuid;
                    Graph graph = _dataContainer.graphAsset.GetGraph(guid);
                    subGraphNode.subGraphGuid = graph.guid;
                }
                
                List<NodeGroup> groups = _dataContainer.currentGraph.nodeGroup;
                NodeGroup group = groups.Find(e => e.Contains(pair.Key));

                if (group is not null)
                {
                    group.RemoveNodeFromGroup(pair.Key, false);
                    group.AddNodeToGroup(node.guid, false);
                }
            }
            
            value = newNodeDictionary;
        }
    }
}