using System;
using System.Collections.Generic;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.FSM;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.Runtime
{
	/// <summary>
	/// Missing Object(스크립트 삭제/변경으로 인해 null이 된 SerializeReference)를 제거할 수 있는 인터페이스입니다.
	/// </summary>
	internal interface IMissingTaskRemovable
	{
#if UNITY_EDITOR
		/// <summary>
		/// Missing Object(null)가 된 참조들을 제거합니다.
		/// 스크립트가 삭제되거나 이름이 변경된 경우 SerializeReference가 null이 됩니다.
		/// </summary>
		/// <returns>제거된 객체 수</returns>
		internal int RemoveMissingTasks();
#endif
	}
	
	
	
	/// <summary>
	/// Missing Object(스크립트 삭제/변경으로 인해 null이 된 SerializeReference)를 정리하는 Visitor입니다.
	/// Node, Service, Condition에서 Missing Object를 감지하고 제거합니다.
	/// </summary>
	internal class MissingObjectCleaner : TaskGraphVisitorBase,
	                                      IVisitPropertyAdapter<GraphDictionary>,
	                                      IVisitPropertyAdapter<NodeDictionary>,
	                                      IVisitPropertyAdapter<List<ServiceBase>>,
	                                      IVisitPropertyAdapter<List<Transition>>,
	                                      IVisitPropertyAdapter<BlackboardData>,
	                                      IVisitPropertyAdapter<BlackboardBasedCondition>
	{
		
		private const string _LOG_PREFIX = "[TaskStreamer]";


		/// <summary> Missing Object가 제거되었는지 여부를 반환합니다. </summary>
		public bool hasCleaned
		{
			get;
			private set;
		}


		//──────────────────────────────────────────────────────────────────
		// GraphDictionary 방문
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
		{
			foreach (KeyValuePair<UGUID, Graph> pair in value)
			{
				switch (pair.Value)
				{
					case BehaviorTree bt: PropertyBag.GetPropertyBag<BehaviorTree>().Accept(this, ref bt); break;

					case StateMachine sm: PropertyBag.GetPropertyBag<StateMachine>().Accept(this, ref sm); break;
				}
			}
		}


		//──────────────────────────────────────────────────────────────────
		// NodeDictionary 방문 - Missing Node 제거
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
		{
			// cachedPair에서 null value를 가진 항목 제거
			int removedCount = value.cachedPair.RemoveAll(static p => p.value == null);

			if (removedCount > 0)
			{
				// Dictionary 재구성
				value.OnAfterDeserialize();

				Debug.Log($"{_LOG_PREFIX} {removedCount}개의 Node를 제거했습니다.");
				hasCleaned = true;
			}

			// IMissingObjectRemovable 인터페이스를 통해 각 노드의 내부 Missing Object 제거
			foreach (NodeBase node in value.Values)
			{
				if (node is null)
				{
					continue;
				}
				
				int removedInternalCount = node.As<IMissingTaskRemovable>()?.RemoveMissingTasks() ?? 0;

				if (removedInternalCount > 0)
				{
					Debug.Log($"{_LOG_PREFIX} '{node.name}'에서 {removedInternalCount}개의 객체를 제거했습니다.");
					hasCleaned = true;
				}
			}

			// 각 노드 내부 방문
			IPropertyBag<Dictionary<UGUID, NodeBase>> bag = PropertyBag.GetPropertyBag<Dictionary<UGUID, NodeBase>>();
			Dictionary<UGUID, NodeBase> dict = value;
			bag.Accept(this, ref dict);
		}


		//──────────────────────────────────────────────────────────────────
		// ServiceList 방문 - Missing Service 제거
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, List<ServiceBase>> context, ref TContainer container, ref List<ServiceBase> value)
		{
			int removedCount = value.RemoveAll(static service => service == null);

			if (removedCount > 0)
			{
				Debug.Log($"{_LOG_PREFIX} {removedCount}개의 Service를 제거했습니다.");
				hasCleaned = true;
			}

			// 유효한 서비스들 내부 방문 (서비스가 가진 BlackboardVariable 등)
			foreach (ServiceBase service in value)
			{
				IPropertyBag bag = PropertyBag.GetPropertyBag(service.GetType());

				if (bag == null)
				{
					continue;
				}

				object reference = service;
				bag.Accept(this, ref reference);
			}
		}


		//──────────────────────────────────────────────────────────────────
		// BlackboardBasedCondition 방문 - Missing Condition 제거
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, BlackboardBasedCondition> context, ref TContainer container, ref BlackboardBasedCondition value)
		{
			if (value.modules == null)
			{
				return;
			}

			int removedCount = value.As<IMissingTaskRemovable>().RemoveMissingTasks();

			if (removedCount > 0)
			{
				Debug.Log($"{_LOG_PREFIX} {removedCount}개의 Condition을 제거했습니다.");
				hasCleaned = true;
			}

			// 유효한 조건들 내부 방문
			foreach (Condition condition in value.modules)
			{
				IPropertyBag bag = PropertyBag.GetPropertyBag(condition.GetType());

				if (bag == null)
				{
					continue;
				}

				object reference = condition;
				bag.Accept(this, ref reference);
			}
		}


		//──────────────────────────────────────────────────────────────────
		// Transition List 방문 - Missing Transition 제거
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, List<Transition>> context, ref TContainer container, ref List<Transition> value)
		{
			// Transition 자체가 null이거나, sourceNode 또는 destinationNode가 null인 경우 제거
			int removedCount = value.RemoveAll(static t => t is null || t.sourceNode is null || t.destinationNode is null);

			if (removedCount > 0)
			{
				Debug.Log($"{_LOG_PREFIX} {removedCount}개의 Transition을 제거했습니다.");
				hasCleaned = true;
			}

			// 각 Transition 내부 방문 (BlackboardBasedCondition Adapter를 통해 Condition 정리)
			foreach (Transition transition in value)
			{
				IPropertyBag<Transition> bag = PropertyBag.GetPropertyBag<Transition>();
				Transition t = transition;
				bag.Accept(this, ref t);
			}
		}


		//──────────────────────────────────────────────────────────────────
		// BlackboardData 방문 - Missing BlackboardVariable 제거
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, BlackboardData> context, ref TContainer container, ref BlackboardData value)
		{
			int removedCount = value.variables.RemoveAll(static v => v == null);

			if (removedCount > 0)
			{
				// Dictionary 재동기화
				value.OnAfterDeserialize();

				Debug.Log($"{_LOG_PREFIX} {removedCount}개의 BlackboardVariable을 제거했습니다.");
				hasCleaned = true;
			}
		}
	}
}