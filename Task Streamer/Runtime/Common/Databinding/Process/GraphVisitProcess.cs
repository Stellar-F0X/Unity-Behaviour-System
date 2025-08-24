using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    internal abstract class GraphVisitProcess : IVisitPropertyAdapter<KeyValuePair<UGUID, Graph>>, IVisitPropertyAdapter<GraphDictionary>
    {
        protected GraphVisitProcess(GraphVisitProcessor processor)
        {
            this.processor = processor;

            _behaviorTreeBag = PropertyBag.GetPropertyBag<BehaviorTree>();
            _stateMachineBag = PropertyBag.GetPropertyBag<StateMachine>();
        }

        private readonly IPropertyBag<BehaviorTree> _behaviorTreeBag;
        private readonly IPropertyBag<StateMachine> _stateMachineBag;
        
        protected readonly GraphVisitProcessor processor;

        
        public virtual void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            IPropertyBag<Dictionary<UGUID, Graph>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, Graph>>();
            Debug.Assert(propertyBag != null, "Property bag not found for GraphDictionary");
            
            Dictionary<UGUID, Graph> dictionaryValue = value as Dictionary<UGUID, Graph>;
            Debug.Assert(dictionaryValue != null, "Dictionary value not found for GraphDictionary");
            
            propertyBag.Accept(processor, ref dictionaryValue);
        }
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, Graph>> context, ref TContainer container, ref KeyValuePair<UGUID, Graph> graphKeyValuePair)
        {
            processor.currentGraph = graphKeyValuePair.Value;
            
            switch (graphKeyValuePair.Value)
            {
                case BehaviorTree behaviorTree: _behaviorTreeBag.Accept(processor, ref behaviorTree); break;

                case StateMachine stateMachine: _stateMachineBag.Accept(processor, ref stateMachine); break;

                //TODO: GOAP

                default: Debug.LogError("Invalid graph type"); break;
            }

            processor.currentGraph = null;
        }
    }
}