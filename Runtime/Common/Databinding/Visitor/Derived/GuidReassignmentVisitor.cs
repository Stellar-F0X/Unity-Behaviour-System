using System.Collections.Generic;
using TaskStreamer.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    /// <summary> GUID를 재할당하는 프로세서를 구현합니다. </summary>
    internal class GuidReassignmentVisitor : GraphVisitorBase, IVisitPropertyAdapter<NodeDictionary>
    {
        /// <summary> 특정 Graph에 대한 Guid 재할당 처리를 지원하는 프로세서 </summary>
        public GuidReassignmentVisitor(GraphContext context) : base(context) { }



        /// <summary> 그래프 속성을 방문하여 Guid를 재할당합니다. </summary>
        /// <param name="context">속성을 방문할 때의 컨텍스트</param>
        /// <param name="container">방문 중인 컨테이너 객체</param>
        /// <param name="value">그래프 딕셔너리 값</param>
        public override void Visit<TContainer>(in VisitContext<TContainer, GraphDictionary> context, ref TContainer container, ref GraphDictionary value)
        {
            value = this.ProcessGraphGuidReassignment(value);
            Debug.Assert(value is not null, "value is not null");
            base.Visit(context, ref container, ref value);
        }


        
        /// <summary> NodeDictionary에 대한 GUID 재할당 처리를 진행합니다. </summary>
        /// <param name="context"> 방문 중인 컨텍스트 정보 </param>
        /// <param name="container"> 컨테이너 객체의 참조 </param>
        /// <param name="value"> 변환 중인 NodeDictionary의 참조 </param>
        public void Visit<TContainer>(in VisitContext<TContainer, NodeDictionary> context, ref TContainer container, ref NodeDictionary value)
        {
            value = this.ProcessNodeGuidReassignment(value);
            Debug.Assert(value is not null, "value is not null");
        }


        /// <summary>GraphDictionary의 GUID를 재할당합니다.</summary>
        /// <param name="originalDictionary">원본 GraphDictionary 객체.</param>
        /// <returns>GUID가 재할당된 새 GraphDictionary 객체.</returns>
        private GraphDictionary ProcessGraphGuidReassignment(GraphDictionary originalDictionary)
        {
            GraphDictionary newDictionary = new GraphDictionary();
            UGUIDDictionary guidMapping = new UGUIDDictionary(); // 이전 GUID -> 새 GUID 매핑

            foreach (KeyValuePair<UGUID, Graph> graphPair in originalDictionary)
            {
                UGUID originalBaseGuid = graphPair.Value.baseGraphGuid;
                UGUID newGuid = UGUID.Create();

                graphPair.Value.guid = newGuid;
                bool isRootGraph = originalBaseGuid.IsEmpty();

                if (isRootGraph)
                {
                    newDictionary.Add(graphPair.Value.guid, graphPair.Value);
                    continue;
                }

                UGUID changedBaseGuid = originalDictionary[originalBaseGuid].guid;
                graphPair.Value.baseGraphGuid = changedBaseGuid;

                if (guidMapping.TryGetValue(changedBaseGuid, out List<UGUID> list))
                {
                    list.Add(graphPair.Value.guid);
                }
                else
                {
                    guidMapping.Add(changedBaseGuid, new List<UGUID>());
                    guidMapping[changedBaseGuid].Add(graphPair.Value.guid);
                }

                newDictionary.Add(graphPair.Value.guid, graphPair.Value);
            }

            _context.graphAsset.graphMap = guidMapping;
            return newDictionary;
        }

        

        /// <summary> 노드 GUID를 재할당하는 처리를 수행합니다. </summary>
        /// <param name="originalDictionary">GUID 재할당이 필요한 노드 사전입니다.</param>
        /// <returns> GUID가 재할당된 새로운 노드 사전입니다.</returns>
        private NodeDictionary ProcessNodeGuidReassignment(NodeDictionary originalDictionary)
        {
            NodeDictionary newNodeDictionary = new NodeDictionary();

            foreach (KeyValuePair<UGUID, NodeBase> pair in originalDictionary)
            {
                NodeBase node = originalDictionary[pair.Key];
                UGUID oldGuid = pair.Key;

                node.guid = UGUID.Create();

                newNodeDictionary.Add(node.guid, node);

                if (node is ISubGraphProvider subGraphNode)
                {
                    Graph graph = _context.graphAsset.GetGraph(subGraphNode.subGraphGuid);
                    subGraphNode.subGraphGuid = graph.guid;
                }

                List<NodeGroup> groups = _context.currentGraph.nodeGroup;
                NodeGroup group = groups.Find(e => e.Contains(oldGuid));

                if (group is null)
                {
                    continue;
                }

                group.RemoveNodeFromGroup(oldGuid, false);
                group.AddNodeToGroup(node.guid, false);
            }

            return newNodeDictionary;
        }
    }
}