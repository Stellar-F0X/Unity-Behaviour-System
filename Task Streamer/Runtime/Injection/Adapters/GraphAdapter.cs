using System.Collections.Generic;
using System.Linq;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    public class GraphAdapter : IVisitPropertyAdapter<Graph>, IVisitPropertyAdapter<GraphDictionary>
    {
        public GraphAdapter(GraphVisitor visitor)
        {
            _visitor = visitor;

            _behaviorTreeBag = PropertyBag.GetPropertyBag<BehaviorTree>();
            _stateMachineBag = PropertyBag.GetPropertyBag<StateMachine>();
        }


        private readonly GraphVisitor _visitor;
        private readonly IPropertyBag<BehaviorTree> _behaviorTreeBag;
        private readonly IPropertyBag<StateMachine> _stateMachineBag;


        public void Visit<TContainer>(in VisitContext<TContainer, Graph> context, ref TContainer container, ref Graph value)
        {
            _visitor.currentGraph = value;

            if (_visitor.debug)
            {
                Debug.Log($"visit {value.name}({value.graphType}) graph.");
            }
            
            switch (value)
            {
                case BehaviorTree behaviorTree: _behaviorTreeBag.Accept(_visitor, ref behaviorTree); break;
                
                case StateMachine stateMachine: _stateMachineBag.Accept(_visitor, ref stateMachine); break;
                
                //TODO: GOAP

                default: Debug.LogError("Invalid graph type"); break;
            }
            
            _visitor.currentGraph = null;
        }
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            IPropertyBag<Dictionary<UGUID, Graph>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, Graph>>();
            Dictionary<UGUID, Graph> dictionaryValue = value as Dictionary<UGUID, Graph>;
            propertyBag.Accept(_visitor, ref dictionaryValue);
            
            foreach (Graph graph in value.Values)
            {
                graph.entry = graph.GetGraphIterator().First();
                Debug.Assert(graph.entry != null, "entry node is null.");
                graph.InitializeOnEnterRuntime(_visitor.taskStreamer);
            }
        }
    }
}