using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// 그래프 노드 간 연결 동작을 관리하기 위해 IEdgeConnectorListener 인터페이스를 구현하는 TaskStreamer 편집기용 기본 클래스.
    /// </summary>
    internal abstract class GraphEdgeConnectorListener : IEdgeConnectorListener
    {
        /// 그래프 뷰에서 엣지 연결 이벤트를 처리하는 리스너를 나타내는 추상 클래스.
        /// 엣지 생성 및 노드 간 연결 관리, 그래프 편집 중의 변경 사항 처리를 담당합니다.
        protected GraphEdgeConnectorListener()
        {
            _pendingEdgesToCreate = new List<Edge>();
            _taskView = TaskStreamerEditor.Instance.taskGraphView;
            _graphViewChange.edgesToCreate = this._pendingEdgesToCreate;
        }

        /// <summary>
        /// 그래프 뷰 내부에서 변경 사항(예: 신규 간선 생성)을 관리하기 위해 사용되는 <see cref="GraphViewChange"/> 구조체의 비공개 인스턴스를 나타냅니다.
        /// </summary>
        /// <remarks>
        /// 주로 노드 간 연결이나 그래프 변경 처리 시 대기 중인 갱신 작업을 추적하고 관리하는 데 활용됩니다.
        /// </remarks>
        private readonly GraphViewChange _graphViewChange;

        /// <summary>
        /// 그래프 엣지 연결 동작을 지원하는 <see cref="TaskGraphView"/> 인스턴스를 참조하는 변수로,
        /// 노드 생성 및 연결과 같은 작업 흐름에서 사용됩니다.
        /// </summary>
        protected readonly TaskGraphView _taskView;



        /// <summary>
        /// 그래프 뷰에서 생성 대기 상태인 간선을 추적하기 위해 사용되는 <see cref="Edge"/> 리스트입니다.
        /// </summary>
        /// <remarks>
        /// 주로 간선 생성 및 연결 프로세스에서 대기 중인 작업을 임시 저장하고 관리하는 데 활용됩니다.
        /// </remarks>
        private List<Edge> _pendingEdgesToCreate;


        /// 엣지가 포트 외부에 드롭되었을 때 호출되며, 새 노드를 생성하고 적절하게 연결합니다.
        /// <param name="edge">드롭된 엣지 객체입니다.</param>
        /// <param name="pos">엣지가 드롭된 그래프 상의 위치입니다.</param>
        public virtual void OnDropOutsidePort(Edge edge, Vector2 pos)
        {
            edge.isGhostEdge = false;

            if (_taskView is null || TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            if (edge.input is not null && edge.input.node is NodeViewBase childView) //Create and link new parent node
            {
                _taskView.OpenContextualMenuWindow(pos, newParentView => this.CreationAndLinkBToA(newParentView, childView, pos));
            }
            else if (edge.output is not null && edge.output.node is NodeViewBase parentView) //Create and link new child node
            {
                _taskView.OpenContextualMenuWindow(pos, newChildView => this.CreationAndLinkAToB(parentView, newChildView, pos));
            }
        }


        // Referenced: https://github.com/thekiwicoder0/UnityBehaviourTreeEditor/blob/main/Editor/NodePort.cs
        // Copyright (c) 2021 Original Author
        // Licensed under the MIT License. See LICENSE file in the root for details.
        /// 그래프 뷰에서 엣지가 드롭되었을 때 처리 로직을 수행합니다.
        /// <param name="graphView">엣지가 드롭된 그래프 뷰 객체입니다.</param>
        /// <param name="edge">드롭된 엣지를 나타냅니다.</param>
        public virtual void OnDrop(GraphView graphView, Edge edge)
        {
            List<GraphElement> edgesToDelete = ListPool<GraphElement>.Get();

            edge.isGhostEdge = false;

            _pendingEdgesToCreate.Clear();
            _pendingEdgesToCreate.Add(edge);

            if (edge.input.capacity == Port.Capacity.Single)
            {
                this.AddToDeleteList(edgesToDelete, edge.input.connections, edge);
            }

            if (edge.output.capacity == Port.Capacity.Single)
            {
                this.AddToDeleteList(edgesToDelete, edge.output.connections, edge);
            }

            if (edgesToDelete.Count > 0)
            {
                graphView.DeleteElements(edgesToDelete);
            }

            ListPool<GraphElement>.Release(edgesToDelete);

            if (graphView.graphViewChanged is not null)
            {
                _pendingEdgesToCreate = graphView.graphViewChanged.Invoke(_graphViewChange).edgesToCreate;
            }

            foreach (Edge e in _pendingEdgesToCreate)
            {
                graphView.AddElement(e);
                edge.input.Connect(e);
                edge.output.Connect(e); 
            }
        }


        /// 지정된 엣지를 제외한 연결된 엣지들을 삭제 목록에 추가합니다.
        /// <param name="edgeToDeleteList">삭제할 엣지를 추가할 리스트입니다.</param>
        /// <param name="edges">삭제를 평가할 엣지들의 컬렉션입니다.</param>
        /// <param name="targetEdge">삭제 대상에서 제외할 엣지입니다.</param>
        protected virtual void AddToDeleteList(List<GraphElement> edgeToDeleteList, IEnumerable<Edge> edges, Edge targetEdge)
        {
            edgeToDeleteList.AddRange(edges.Where(edge => edge is not null && edge != targetEdge));
        }


        /// <summary>
        /// 기존 노드와 새로 생성된 노드를 연결하는 추상 메서드.
        /// 행동 트리(Behavior Tree)에서는 부모-자식 관계를, 유한 상태 기계(Finite State Machine)에서는 상태 전환 관계를 설정.
        /// </summary>
        /// <param name="sourceView">연결의 출발점이 되는 기존 노드.</param>
        /// <param name="newView">새로 생성된 후 연결될 노드.</param>
        /// <param name="position">그래프 뷰에서 새 노드가 배치되는 위치.</param>
        protected abstract void CreationAndLinkAToB(NodeViewBase sourceView, NodeViewBase newView, Vector2 position);


        /// <summary>
        /// 새로운 노드를 생성하고 기존 노드와의 관계를 연결합니다.
        /// </summary>
        /// <param name="newView">생성될 새로운 노드를 나타내는 뷰 객체.</param>
        /// <param name="sourceView">기존/원본 노드를 나타내는 뷰 객체.</param>
        /// <param name="position">새로운 노드가 생성될 위치.</param>
        protected abstract void CreationAndLinkBToA(NodeViewBase newView, NodeViewBase sourceView, Vector2 position);
    }
}