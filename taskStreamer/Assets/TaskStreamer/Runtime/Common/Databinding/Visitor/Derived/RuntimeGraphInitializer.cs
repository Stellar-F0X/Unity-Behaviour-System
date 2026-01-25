using System.Collections.Generic;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.FSM;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Runtime
{
	/// <summary>
	/// 런타임에서 그래프를 초기화하는 흐름입니다.
	///
	/// [흐름]
	/// GraphDictionary 방문
	///     → Graph 방문
	///         → NodeDictionary 방문 (OnInstantiate 호출)
	///             → Node 방문
	///                 → BlackboardVariable 바인딩
	///                 → ServiceList 방문
	///         → Transition 방문
	///     → Graph 런타임 초기화
	/// </summary>
	internal class RuntimeGraphInitializer : ReadableVisitorBase,
	                                         IVisitPropertyAdapter<GraphDictionary>,
	                                         IVisitPropertyAdapter<KeyValuePair<UGUID, Graph>>,
	                                         IVisitPropertyAdapter<NodeDictionary>,
	                                         IVisitPropertyAdapter<KeyValuePair<UGUID, NodeBase>>,
	                                         IVisitPropertyAdapter<Transition>,
	                                         IVisitPropertyAdapter<List<ServiceBase>>,
	                                         IVisitContravariantPropertyAdapter<BlackboardVariable>
	{
		private readonly GraphContext _context;

		public RuntimeGraphInitializer(GraphContext context)
		{
			_context = context;
		}


		//──────────────────────────────────────────────────────────────────
		// GraphDictionary 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
		{
			// 1) 각 Graph 방문
			Dictionary<UGUID, Graph> dict = value;
			PropertyBag.GetPropertyBag<Dictionary<UGUID, Graph>>().Accept(this, ref dict);

			// 2) 방문 완료 후 런타임 초기화
			foreach (Graph graph in value.Values)
			{
				Debug.Assert(graph.entry != null, "entry node is null.");
				graph.InitializeOnEnterRuntime(_context.taskStreamer);
			}
		}


		//──────────────────────────────────────────────────────────────────
		// Graph 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, Graph>> context, ref TContainer container, ref KeyValuePair<UGUID, Graph> value)
		{
			_context.currentGraph = value.Value;

			// Graph 내부 방문 (Node, Transition 등)
			switch (value.Value)
			{
				case BehaviorTree bt: PropertyBag.GetPropertyBag<BehaviorTree>().Accept(this, ref bt); break;

				case StateMachine sm: PropertyBag.GetPropertyBag<StateMachine>().Accept(this, ref sm); break;
			}

			_context.currentGraph = null;
		}


		//──────────────────────────────────────────────────────────────────
		// NodeDictionary 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
		{
			// 1) 모든 노드 인스턴스화
			foreach (KeyValuePair<UGUID, NodeBase> pair in value)
			{
				pair.Value.OnInstantiate();
			}

			// 2) 각 노드 내부 방문 (BBVariable, Service 등)
			Dictionary<UGUID, NodeBase> dict = value;
			PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>().Accept(this, ref dict);
		}


		//──────────────────────────────────────────────────────────────────
		// Node 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, NodeBase>> context, ref TContainer container, ref KeyValuePair<UGUID, NodeBase> value)
		{
			NodeBase node = value.Value;

			IPropertyBag bag = PropertyBag.GetPropertyBag(node.GetType());
			Assert.IsNotNull(bag, $"Property bag not found for {node.name}");

			object reference = node;
			bag.Accept(this, ref reference);
		}


		//──────────────────────────────────────────────────────────────────
		// BlackboardVariable 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
		{
			if (context.Property.IsReadOnly)
			{
				Debug.LogError($"'{typeof(TContainer)}.{context.Property.Name}' is read-only.");
				return;
			}

			Assert.IsNotNull(value);

			// 런타임 바인딩: Local → 복제, Shared → Blackboard에서 연결
			BlackboardVariable bound = BlackboardVariableHelper.BindForRuntime(value, _context);
			context.Property.SetValue(ref container, bound);
		}


		//──────────────────────────────────────────────────────────────────
		// Transition 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
		{
			if (value.conditions.modules.Count == 0)
			{
				return;
			}

			// Condition 내부의 BBVariable 방문
			List<Condition> conditions = value.conditions.modules;
			PropertyBag.GetPropertyBag<List<Condition>>().Accept(this, ref conditions);
		}


		//──────────────────────────────────────────────────────────────────
		// ServiceList 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, List<ServiceBase>> context, ref TContainer container, ref List<ServiceBase> value)
		{
			foreach (ServiceBase service in value)
			{
				// 1) Service에 소유 노드 연결
				service.node = container as BehaviorNodeBase;
				Assert.IsNotNull(service.node);

				// 2) Service 내부 방문 (BBVariable 등)
				IPropertyBag bag = PropertyBag.GetPropertyBag(service.GetType());
				if (bag is null)
				{
					Debug.LogError($"Property bag not found for {service.GetType().Name}");
					continue;
				}

				object reference = service;
				bag.Accept(this, ref reference);
			}
		}
	}
}