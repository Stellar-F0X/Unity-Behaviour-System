using System.Collections.Generic;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// 그래프 뷰를 제어하는 추상 기본 클래스
    /// Behavior Tree(BT)와 Finite State Machine(FSM) 그래프 타입을 처리합니다.
    /// </summary>
    internal abstract class GraphViewBase
    {
        /// <summary>
        /// 그래프 타입별 프로세서 인스턴스 배열 (BT: 0, FSM: 1)
        /// 싱글톤 패턴으로 각 그래프 타입당 하나의 인스턴스만 유지합니다.
        /// </summary>
        private static GraphViewBase[] _ProcessorInstances = new GraphViewBase[2];


        /// <summary>
        /// 스크립트 리로드 시 프로세서 인스턴스들을 초기화합니다.
        /// Unity 에디터에서 스크립트가 다시 컴파일될 때 자동으로 호출됩니다.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ResetViewInstancesOnScriptReload()
        {
            _ProcessorInstances[0] = null; // BT 프로세서 초기화
            _ProcessorInstances[1] = null; // FSM 프로세서 초기화
        }


        /// <summary>
        /// 그래프 타입에 따라 적절한 GraphViewControl 인스턴스를 생성하거나 반환합니다.
        /// 팩토리 메서드 패턴을 사용하여 타입별 컨트롤러를 생성합니다.
        /// </summary>
        /// <param name="graph">처리할 그래프 객체</param>
        /// <returns>그래프 타입에 맞는 GraphViewControl 인스턴스</returns>
        public static GraphViewBase CreateGraphView(Graph graph)
        {
            // 이미 생성된 인스턴스가 있으면 재사용
            if (_ProcessorInstances is not null && _ProcessorInstances[(int)graph.graphType] != null)
            {
                return _ProcessorInstances[(int)graph.graphType];
            }

            GraphViewBase result = null;

            // 그래프 타입에 따라 적절한 컨트롤러 생성
            switch (graph.graphType)
            {
                case GraphType.BT: result = new BTView(); break;

                case GraphType.FSM: result = new FSMView(); break;
            }

            // 생성된 인스턴스를 캐시하고 반환
            _ProcessorInstances[(int)graph.graphType] = result;
            return result;
        }
        

        /// <summary>
        /// 그래프에서 노드를 삭제합니다.
        /// 서브그래프 노드의 경우 연관된 하위 그래프들도 함께 삭제합니다.
        /// </summary>
        /// <param name="graph">대상 그래프</param>
        /// <param name="targetNode">삭제할 노드</param>
        public void DeleteNodeFromGraph(Graph graph, NodeViewBase targetNode)
        {
            TaskStreamerEditor.Instance.inspectorView.ClearInspector(true);
            
            targetNode.OnRemoved();
            
            // 서브그래프 노드인 경우 특별 처리
            if (targetNode.targetNode is ISubGraphProvider subGraphNode)
            {
                UGUID targetGuid = subGraphNode.subGraphGuid;
                Graph foundSubGraph = TaskStreamerEditor.Instance.graphAsset.GetGraph(targetGuid);
                Debug.Assert(foundSubGraph != null, $"Graph is null. guid : {targetGuid}");

                // 서브그래프와 그 하위 그래프들을 재귀적으로 삭제
                foundSubGraph.RemoveSelfAndSubGraphs();
            }

            // 노드 자체도 삭제
            graph.DeleteAndRemoveNodeFromList(targetNode.targetNode);
        }


        /// <summary>
        /// 노드 위치 변경 알림을 처리합니다.
        /// 기본 구현은 비어있으며, 필요에 따라 하위 클래스에서 오버라이드합니다.
        /// </summary>
        /// <param name="graphView">그래프 뷰</param>
        /// <param name="elements">위치가 변경된 그래프 요소들</param>
        public virtual void NotifyNodePositionChanged(TaskGraphView graphView, List<GraphElement> elements) { }


        /// <summary>
        /// 두 노드를 엣지로 연결을 시도합니다.
        /// </summary>
        /// <param name="graphView">그래프 뷰</param>
        /// <param name="sourceView">연결 대상 노드</param>
        /// <param name="targetView">연결할 노드</param>
        /// <returns>연결 성공 여부</returns>
        public abstract bool TryConnectNodesByEdge(TaskGraphView graphView, NodeViewBase sourceView, NodeViewBase targetView);


        /// <summary>
        /// 그래프의 모든 노드를 생성하고 연결합니다.
        /// 주로 그래프 로딩 시 사용됩니다.
        /// </summary>
        /// <param name="graphView">그래프 뷰</param>
        /// <param name="graph">로딩할 그래프 데이터</param>
        public abstract void CreateAndConnectNodes(TaskGraphView graphView, Graph graph);


        /// <summary>
        /// 선택된 요소들을 필터링합니다.
        /// 특정 노드들(예: Root 노드)을 선택에서 제외할 때 사용됩니다.
        /// </summary>
        /// <param name="selection">선택된 요소들의 리스트</param>
        public abstract void FilterSelectionElements(List<ISelectable> selection);


        /// <summary>
        /// 노드 데이터로부터 노드 뷰를 재생성합니다.
        /// 그래프 로딩 시 각 노드에 대해 호출됩니다.
        /// </summary>
        /// <param name="node">소스 노드 데이터</param>
        /// <returns>생성된 노드 뷰</returns>
        public abstract NodeViewBase RecreateNodeViewOnLoad(NodeBase node);


        /// <summary>
        /// 엣지를 통해 노드들의 연결을 해제합니다.
        /// </summary>
        /// <param name="graph">대상 그래프</param>
        /// <param name="edge">해제할 엣지</param>
        public abstract void DisconnectNodesByEdge(Graph graph, Edge edge);


        /// <summary>
        /// 여러 엣지를 통해 노드들을 연결합니다.
        /// 주로 복사/붙여넣기나 실행 취소 작업에서 사용됩니다.
        /// </summary>
        /// <param name="view">그래프 뷰</param>
        /// <param name="graph">대상 그래프</param>
        /// <param name="edges">연결할 엣지들의 리스트</param>
        public abstract void ConnectNodesByEdges(TaskGraphView view, Graph graph, List<Edge> edges);


        /// <summary>
        /// 그래프 타입에 맞는 노드 생성 창을 생성합니다.
        /// 하위 클래스에서 적절한 창 타입을 반환해야 합니다.
        /// </summary>
        /// <returns>생성된 노드 생성 창</returns>
        public abstract BindingWindow CreateGraphNodeCreationWindow(TaskGraphView graphView);
    }
}