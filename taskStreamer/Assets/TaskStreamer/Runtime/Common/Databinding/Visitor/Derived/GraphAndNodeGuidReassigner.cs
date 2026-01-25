using System.Collections.Generic;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.FSM;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;

namespace TaskStreamer.Runtime
{
#if UNITY_EDITOR
	/// <summary>
	/// 그래프 복제 시 모든 GUID를 재할당하는 흐름입니다.
	///
	/// [흐름]
	/// GraphDictionary 방문
	///     → Graph GUID 재할당
	///     → Graph 방문
	///         → NodeDictionary 방문
	///             → Node GUID 재할당
	///             → SubGraph GUID 갱신
	///             → Service GUID 재할당
	///             → NodeGroup 갱신
	/// </summary>
	internal class GraphAndNodeGuidReassigner : ReadableVisitorBase,
	                                            IVisitPropertyAdapter<NodeDictionary>,
	                                            IVisitPropertyAdapter<GraphDictionary>,
	                                            IVisitPropertyAdapter<KeyValuePair<UGUID, Graph>>
	{

		public GraphAndNodeGuidReassigner(GraphContext context)
		{
			_context = context;
		}

		
		private readonly GraphContext _context;

		//──────────────────────────────────────────────────────────────────
		// GraphDictionary 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
		{
			// 1) Graph GUID 재할당
			value = this.ReassignGraphGuids(value);

			// 2) 각 Graph 내부 방문
			Dictionary<UGUID, Graph> dict = value;
			PropertyBag.GetPropertyBag<Dictionary<UGUID, Graph>>().Accept(this, ref dict);
		}


		private GraphDictionary ReassignGraphGuids(GraphDictionary original)
		{
			GraphDictionary result = new GraphDictionary();
			UGUIDDictionary guidMapping = new UGUIDDictionary();

			foreach (KeyValuePair<UGUID, Graph> pair in original)
			{
				Graph graph = pair.Value;
				UGUID originalBaseGuid = graph.baseGraphGuid;

				// 새 GUID 할당
				graph.guid = UGUID.Create();

				// 루트 그래프인 경우
				if (originalBaseGuid.IsEmpty())
				{
					result.Add(graph.guid, graph);
					continue;
				}

				// 서브 그래프인 경우: baseGraphGuid 갱신
				UGUID newBaseGuid = original[originalBaseGuid].guid;
				graph.baseGraphGuid = newBaseGuid;

				// 매핑 기록
				if (guidMapping.TryGetValue(newBaseGuid, out List<UGUID> list) == false)
				{
					list = new List<UGUID>();
					guidMapping.Add(newBaseGuid, list);
				}

				list.Add(graph.guid);
				result.Add(graph.guid, graph);
			}

			_context.graphAsset.graphMap = guidMapping;
			return result;
		}


		//──────────────────────────────────────────────────────────────────
		// Graph 흐름
		//──────────────────────────────────────────────────────────────────

		public void Visit<TContainer>(in VisitContext<TContainer, KeyValuePair<UGUID, Graph>> context, ref TContainer container, ref KeyValuePair<UGUID, Graph> value)
		{
			_context.currentGraph = value.Value;

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
			// Node GUID 재할당
			value = ReassignNodeGuids(value);

			// 방문 계속
			context.ContinueVisitation(ref container, ref value);
		}


		private NodeDictionary ReassignNodeGuids(NodeDictionary original)
		{
			NodeDictionary result = new NodeDictionary();
			List<NodeGroup> groups = _context.currentGraph.nodeGroup;

			foreach (KeyValuePair<UGUID, NodeBase> pair in original)
			{
				NodeBase node = pair.Value;
				UGUID oldGuid = pair.Key;

				// 1) Node GUID 재할당
				node.guid = UGUID.Create();
				result.Add(node.guid, node);

				// 2) SubGraph 참조 갱신
				if (node is ISubGraph subGraphNode)
				{
					Graph subGraph = _context.graphAsset.GetGraph(subGraphNode.subGraphGuid);
					subGraphNode.subGraphGuid = subGraph.guid;
				}

				// 3) Service GUID 재할당
				if (node is BehaviorNodeBase behavior)
				{
					behavior.services.ForEach(s => s.guid = UGUID.Create());
				}

				// 4) NodeGroup 갱신
				NodeGroup group = groups.Find(g => g.Contains(oldGuid));

				if (group != null)
				{
					group.RemoveNodeFromGroup(oldGuid, false);
					group.AddNodeToGroup(node.guid, false);
				}
			}

			return result;
		}
	}
#endif
}