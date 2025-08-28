using System.Collections.Generic;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    /// <summary> 런타임에서 그래프 탐색 및 처리를 활성화하는 클래스 </summary>
    internal class GraphRuntimeInitializeVisitor : GraphVisitorBase,
                                                   IVisitPropertyAdapter<NodeDictionary>,
                                                   IVisitPropertyAdapter<KeyValuePair<UGUID, NodeBase>>,
                                                   IVisitPropertyAdapter<Transition>,
                                                   IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        /// <summary> 런타임 시 노드 및 그래프의 인스턴스화를 처리하는 클래스 </summary>
        public GraphRuntimeInitializeVisitor(GraphContext context) : base(context) { }



        /// <summary> NodeDictionary 속성을 방문하며 처리합니다. </summary>
        /// <param name="context">방문 컨텍스트를 나타냅니다.</param>
        /// <param name="container">방문 중인 컨테이너 객체입니다.</param>
        /// <param name="value">방문 중인 NodeDictionary 값입니다.</param>
        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            foreach (KeyValuePair<UGUID, NodeBase> nodePairs in value)
            {
                nodePairs.Value.OnInstantiate();
            }

            IPropertyBag<Dictionary<UGUID, NodeBase>> propertyBag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
            Dictionary<UGUID, NodeBase> dictionaryValue = (Dictionary<UGUID, NodeBase>)value;
            propertyBag.Accept(this, ref dictionaryValue);
        }



        /// <summary> NodeDictionary 방문을 처리합니다. </summary>
        /// <param name="context">현재 방문 컨텍스트입니다.</param>
        /// <param name="container">방문 중인 컨테이너입니다.</param>
        /// <param name="value">방문 중인 NodeDictionary 값입니다.</param>
        public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, NodeBase>> context, ref TContainer container, ref KeyValuePair<UGUID, NodeBase> value)
        {
            //굳이 이렇게 하는 이유는 PropertyVisit이 Dictionary<TKey, TValue>에서 Key에 방문하는 것을 무시를 위해서.
            IPropertyBag bag = PropertyBag.GetPropertyBag(value.Value.GetType());
            Debug.Assert(bag != null, $"Property bag not found for {value.Value.name}");

            object reference = value.Value;
            bag.Accept(this, ref reference);
        }



        /// <summary> NodeDictionary 타입의 프로퍼티를 방문하는 메서드입니다. </summary>
        /// <param name="context">방문 컨텍스트입니다.</param>
        /// <param name="container">컨테이너 객체입니다.</param>
        /// <param name="value">방문 대상 노드 딕셔너리입니다.</param>
        public override void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            base.Visit(context, ref container, ref value);

            foreach (Graph graph in value.Values)
            {
                Debug.Assert(graph.entry != null, "entry node is null.");
                graph.InitializeOnEnterRuntime(_context.taskStreamer);
            }
        }



        /// <summary> NodeDictionary 타입의 프로퍼티를 방문하여 처리합니다. </summary>
        /// <param name="context"> 방문 컨텍스트입니다. </param>
        /// <param name="container"> 해당 컨테이너입니다. </param>
        /// <param name="value"> NodeDictionary 값입니다. </param>
        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (context.Property.IsReadOnly)
            {
                Debug.LogError($"'{typeof(TContainer)}.{context.Property.Name}' is read-only and cannot be modified.");
                return;
            }
            
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
            if (_context.blackboard == null || _context.blackboard.count == 0)
            {
                Debug.LogError($"잘못된 {typeof(TContainer)}의 {context.Property} 필드입니다.");
                return;
            }

            //Runtime instantiated 객체가 작동 중임은 런타임 blackboard가 할당됐음을 의미하니, runtime bb variable을 찾아서 할당해준다.
            BlackboardVariable shared = ObjectFactory.CreateSharedBlackboardVariable(value.implementedType, _context.blackboard, value.guid);
            shared.usage = value.usage;
            Debug.Assert(shared != null, "Variable not found in blackboard.");
            context.Property.SetValue(ref container, shared);
        }



        /// <summary> Transition의 ConditionModule이 없으면 BBVariable을 할당하지 않도록 처리합니다. </summary>
        /// <param name="context"> 방문 컨텍스트입니다. </param>
        /// <param name="container"> 방문 대상 데이터 컨테이너입니다. </param>
        /// <param name="value"> Transition 객체의 참조 값입니다. </param>
        public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
        {
            //ConditionModule이 없다면 BBVariable을 할당하지 않아도 되므로 Early Return.
            if (value.conditions.modules.Count == 0)
            {
                return;
            }

            IPropertyBag<List<Condition>> bag = PropertyBag.GetPropertyBag<List<Condition>>();
            List<Condition> conditions = value.conditions.modules;
            bag.Accept(this, ref conditions);
        }
    }
}