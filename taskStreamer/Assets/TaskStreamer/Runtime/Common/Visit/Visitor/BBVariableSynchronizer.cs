using System;
using System.Collections.Generic;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.FSM;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Runtime
{
#if UNITY_EDITOR
	/// <summary>
	/// Blackboard 변경 시 BlackboardVariable을 동기화하는 흐름입니다.
	///
	/// [흐름]
	/// GraphDictionary 방문
	///     → Graph 방문
	///         → NodeDictionary 방문
	///             → BlackboardVariable 동기화
	///             → ServiceList 방문
	///         → Condition 동기화
	/// </summary>
	internal class BBVariableSynchronizer : TaskGraphVisitorBase,
	                                        IVisitPropertyAdapter<GraphDictionary>,
	                                        IVisitPropertyAdapter<NodeDictionary>,
	                                        IVisitPropertyAdapter<BlackboardBasedCondition>,
	                                        IVisitPropertyAdapter<List<ServiceBase>>,
	                                        IVisitContravariantPropertyAdapter<BlackboardVariable>
	{
		public BBVariableSynchronizer(GraphVisitContext visitContext)
		{
			_visitContext = visitContext;
		}


		private readonly GraphVisitContext _visitContext;



		//──────────────────────────────────────────────────────────────────
		// GraphDictionary 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
		{
			foreach (KeyValuePair<UGUID, Graph> dict in value)
			{
				_visitContext.currentGraph = dict.Value;

				switch (dict.Value)
				{
					case BehaviorTree bt: PropertyBag.GetPropertyBag<BehaviorTree>().Accept(this, ref bt); break;

					case StateMachine sm: PropertyBag.GetPropertyBag<StateMachine>().Accept(this, ref sm); break;
				}

				_visitContext.currentGraph = null;
			}
		}


		//──────────────────────────────────────────────────────────────────
		// NodeDictionary 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
		{
			Dictionary<UGUID, NodeBase> dict = value;
			PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>().Accept(this, ref dict);
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

			// 유효하지 않은 Shared Variable → 새 Local Variable로 교체
			BlackboardVariable synced = _visitContext.blackboard.SyncWithBlackboard(value);

			if (synced != value)
			{
				context.Property.SetValue(ref container, synced);
			}
		}


		//──────────────────────────────────────────────────────────────────
		// Condition 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, BlackboardBasedCondition> context,
		                              ref TContainer container,
		                              ref BlackboardBasedCondition value
		)
		{
			if (value.modules is null || value.modules.Count == 0)
			{
				return;
			}

			// 각 Condition의 lVariable / rVariable 동기화
			foreach (Condition condition in value.modules)
			{
				if (condition is null)
				{
					continue;
				}

				Type conditionType = condition.GetType();
				IPropertyBag bag = PropertyBag.GetPropertyBag(conditionType);
				Assert.IsNotNull(bag, $"Property bag not found for {conditionType.Name}");

				object reference = condition;
				bag.Accept(this, ref reference);
			}
		}


		//──────────────────────────────────────────────────────────────────
		// ServiceList 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, List<ServiceBase>> context, ref TContainer container, ref List<ServiceBase> value)
		{
			foreach (ServiceBase service in value)
			{
				Type serviceType = service.GetType();
				IPropertyBag bag = PropertyBag.GetPropertyBag(serviceType);
				Assert.IsNotNull(bag, $"Property bag not found for {serviceType.Name}");

				object reference = service;
				bag.Accept(this, ref reference);
			}
		}
	}
#endif
}