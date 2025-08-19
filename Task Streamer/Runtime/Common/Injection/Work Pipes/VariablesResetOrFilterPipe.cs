using System.Collections.Generic;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;

namespace TaskStreamer.Injection
{
    /// <summary> Blackboard 교체될 때, 이미 등록되어 있는 BlackboardVariable을 해제하는 용도로 사용되는 객체. </summary>
    internal class VariablesResetOrFilterPipe : GraphPipe,
                                        IVisitPropertyAdapter<NodeDictionary>,
                                        IVisitPropertyAdapter<Transition>,
                                        IVisitPropertyAdapter<BlackboardBasedCondition>,
                                        IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public VariablesResetOrFilterPipe(GraphTraveler traveler) : base(traveler) { }



        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)value;
            propertyBag.Accept(Traveler, ref dictionaryValue);
        }



        public void Visit<TContainer>(in VisitContext<TContainer, BlackboardBasedCondition> context, ref TContainer container, ref BlackboardBasedCondition value)
        {
            if (value.modules.Count == 0)
            {
                return;
            }

            value.modules.Clear();
        }



        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (value is null || value.isGlobal == false)
            {
                return;
            }

            value.variable = null;
        }



        public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
        {
            if (value.conditions.modules.Count == 0)
            {
                return;
            }

            value.conditional = false;
            value.conditions.modules.Clear();
        }
    }
}