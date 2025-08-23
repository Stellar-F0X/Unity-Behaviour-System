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
    internal class RuntimeInstantiationPipe : GraphPipe,
                                              IVisitPropertyAdapter<NodeDictionary>,
                                              IVisitPropertyAdapter<KeyValuePair<UGUID, NodeBase>>,
                                              IVisitPropertyAdapter<Transition>,
                                              IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public RuntimeInstantiationPipe(GraphTraveler traveler) : base(traveler) { }



        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            foreach (KeyValuePair<UGUID, NodeBase> nodePairs in value)
            {
                nodePairs.Value.name += "(Clone)";
                nodePairs.Value.OnInstantiate();
            }

            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)value;
            propertyBag.Accept(traveler, ref dictionaryValue);
        }



        public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, NodeBase>> context, ref TContainer container, ref KeyValuePair<UGUID, NodeBase> pair)
        {
            //굳이 이렇게 하는 이유는 PropertyVisit이 Dictionary<TKey, TValue>에서 Key에 방문하는 것을 무시를 위해서.
            IPropertyBag bag = PropertyBag.GetPropertyBag(pair.Value.GetType());
            Debug.Assert(bag != null, $"Property bag not found for {pair.Value.name}");

            object reference = pair.Value; //어차피 노드는 항상 Class 타입이므로, object로 형변환해도 Boxing/Unboxing은 문제 없음.
            bag.Accept(traveler, ref reference);
        }



        public override void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            base.Visit(context, ref container, ref value);

            foreach (Graph graph in value.Values)
            {
                Debug.Assert(graph.entry != null, "entry node is null.");
                graph.InitializeOnEnterRuntime(traveler.taskStreamer);
            }
        }



        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (value?.variable is null || traveler.blackboard == null || traveler.blackboard.count == 0)
            {
                return;
            }

            if (value.isGlobal == false)
            {
                context.Property.SetValue(ref container, value.Duplicate());
                return;
            }

            Variable foundVariable = traveler.blackboard.FindVariable(value.guid);
            Debug.Assert(foundVariable != null, "Variable not found in blackboard.");

            BlackboardVariable blackboardVariable = value.Duplicate(); 
            blackboardVariable.variable = foundVariable;
            context.Property.SetValue(ref container, blackboardVariable);
        }


        public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
        {
            if (value.conditions.modules.Count == 0) //ConditionModule이 없다면 BBVariable을 할당하지 않아도 되므로 Early Return.
            {
                return;
            }

            IPropertyBag<List<Condition>> bag = PropertyBag.GetPropertyBag<List<Condition>>();
            List<Condition> conditions = value.conditions.modules;
            bag.Accept(traveler, ref conditions);
        }
    }
}