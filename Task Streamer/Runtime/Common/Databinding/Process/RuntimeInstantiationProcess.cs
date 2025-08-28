using System.Collections.Generic;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    internal class RuntimeInstantiationProcess : GraphVisitProcess,
                                              IVisitPropertyAdapter<NodeDictionary>,
                                              IVisitPropertyAdapter<KeyValuePair<UGUID, NodeBase>>,
                                              IVisitPropertyAdapter<Transition>,
                                              IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public RuntimeInstantiationProcess(GraphVisitProcessor processor) : base(processor) { }



        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            foreach (KeyValuePair<UGUID, NodeBase> nodePairs in value)
            {
                nodePairs.Value.OnInstantiate();
            }

            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)value;
            propertyBag.Accept(processor, ref dictionaryValue);
        }



        public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, NodeBase>> context, ref TContainer container, ref KeyValuePair<UGUID, NodeBase> pair)
        {
            //굳이 이렇게 하는 이유는 PropertyVisit이 Dictionary<TKey, TValue>에서 Key에 방문하는 것을 무시를 위해서.
            IPropertyBag bag = PropertyBag.GetPropertyBag(pair.Value.GetType());
            Debug.Assert(bag != null, $"Property bag not found for {pair.Value.name}");

            object reference = pair.Value; 
            bag.Accept(processor, ref reference);
        }



        public override void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            base.Visit(context, ref container, ref value);

            foreach (Graph graph in value.Values)
            {
                Debug.Assert(graph.entry != null, "entry node is null.");
                graph.InitializeOnEnterRuntime(processor.taskStreamer);
            }
        }
        


        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            //정상적으로 동작한다면, value가 null일 수 없다.
            if (value is null)
            {
                Debug.LogError($"잘못된 {typeof(TContainer)}의 {context.Property} 필드입니다.");
                return;
            }
            
            //null도, shared variable도, 아니라면 그냥 필드에 새 로컬(Local) 객체만 할당 해주면 된다.
            if (value.isShared == false)
            {
                context.Property.SetValue(ref container, value.Duplicate());
                return;
            }
            
            //shared variable이지만, blackboard가 null이라면 제때 제거되지 않은 잘못된 객체이므로 경고를 띄운다. 
            if (processor.blackboard == null || processor.blackboard.count == 0)
            {
                Debug.LogError($"잘못된 {typeof(TContainer)}의 {context.Property} 필드입니다.");
                return;
            }

            //Runtime instantiated 객체가 작동 중임은 런타임 blackboard가 할당됐음을 의미하니, runtime bb variable을 찾아서 할당해준다.
            BlackboardVariable shared = ObjectFactory.CreateSharedBlackboardVariable(value.implementedType, processor.blackboard, value.guid);
            shared.usage = value.usage;
            Debug.Assert(shared != null, "Variable not found in blackboard.");
            context.Property.SetValue(ref container, shared);
        }

        

        public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
        {
            //ConditionModule이 없다면 BBVariable을 할당하지 않아도 되므로 Early Return.
            if (value.conditions.modules.Count == 0)
            {
                return;
            }

            IPropertyBag<List<Condition>> bag = PropertyBag.GetPropertyBag<List<Condition>>();
            List<Condition> conditions = value.conditions.modules;
            bag.Accept(processor, ref conditions);
        }
    }
}