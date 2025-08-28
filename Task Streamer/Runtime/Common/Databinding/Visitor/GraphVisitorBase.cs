using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    internal abstract class GraphVisitorBase : ReadableVisitorBase, IVisitPropertyAdapter<KeyValuePair<UGUID, Graph>>, IVisitPropertyAdapter<GraphDictionary>
    {
        protected GraphVisitorBase(GraphContext context)
        {
            this._context = context;

            _behaviorTreeBag = PropertyBag.GetPropertyBag<BehaviorTree>();
            _stateMachineBag = PropertyBag.GetPropertyBag<StateMachine>();
        }

        private readonly IPropertyBag<BehaviorTree> _behaviorTreeBag;
        private readonly IPropertyBag<StateMachine> _stateMachineBag;
        
        protected readonly GraphContext _context;

        
        
        public virtual void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            IPropertyBag<Dictionary<UGUID, Graph>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, Graph>>();
            Debug.Assert(propertyBag != null, "Property bag not found for GraphDictionary");
            
            Dictionary<UGUID, Graph> dictionaryValue = value as Dictionary<UGUID, Graph>;
            Debug.Assert(dictionaryValue != null, "Dictionary value not found for GraphDictionary");
            
            propertyBag.Accept(this, ref dictionaryValue);
        }
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, Graph>> context, ref TContainer container, ref KeyValuePair<UGUID, Graph> graphKeyValuePair)
        {
            _context.currentGraph = graphKeyValuePair.Value;
            
            switch (graphKeyValuePair.Value)
            {
                case BehaviorTree behaviorTree: _behaviorTreeBag.Accept(this, ref behaviorTree); break;

                case StateMachine stateMachine: _stateMachineBag.Accept(this, ref stateMachine); break;

                //TODO: GOAP

                default: Debug.LogError("Invalid graph type"); break;
            }

            _context.currentGraph = null;
        }
    }
}