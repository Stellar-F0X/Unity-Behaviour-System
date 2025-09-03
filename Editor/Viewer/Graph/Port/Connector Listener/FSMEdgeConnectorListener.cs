using TaskStreamer.FSM;
using UnityEngine;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// FSMEdgeConnectorListener 클래스는 상태 머신(FSM)의 그래프 시각화에서 엣지 연결 처리를 담당합니다.
    /// 추상 클래스 GraphEdgeConnectorListener를 확장하여 상태 간 연결 및 관리 로직을 구현합니다.
    /// </summary>
    /// <remarks>
    /// FSMView 및 StateMachine 클래스를 활용하여 상태 연결 구조를 구성하고,
    /// 입력된 규칙 기반의 처리 흐름을 보장합니다.
    /// </remarks>
    internal class FSMEdgeConnectorListener : GraphEdgeConnectorListener
    {
        /// <summary>
        /// 소스 노드를 기존 연결에서 분리하고 새로 생성된 노드와 연결한 뒤, 그래프 엣지를 통해 연결을 설정합니다.
        /// </summary>
        /// <param name="sourceView">기존 연결에서 분리되고 새 노드와 연결될 소스 노드입니다.</param>
        /// <param name="newView">새롭게 생성되어 소스 노드와 연결될 노드입니다.</param>
        /// <param name="position">그래프 뷰에서 새 노드가 생성될 위치입니다.</param>
        protected override void CreationAndLinkAToB(NodeViewBase sourceView, NodeViewBase newView, Vector2 position)
        {
            ((FSMView)_taskView.graphView).TryDisconnectSourceToOriginal(sourceView);

            StateMachine stateMachine = TaskStreamerEditor.Instance.currentGraph as StateMachine;
            Debug.Assert(stateMachine is not null, "stateMachine is null");

            stateMachine.ConnectStates((StateBase)sourceView.targetNode, (StateBase)newView.targetNode);

            _taskView.graphView.TryConnectNodesByEdge(_taskView, sourceView, newView);
        }


        /// <summary>
        /// 원본 소스 노드와 새 노드를 연결하여 그래프에 새로운 연결을 생성합니다.
        /// </summary>
        /// <param name="newView">새로 생성된 노드의 뷰입니다.</param>
        /// <param name="sourceView">연결될 소스 노드의 뷰입니다.</param>
        /// <param name="position">연결이 그래프에서 이루어지는 위치입니다.</param>
        protected override void CreationAndLinkBToA(NodeViewBase newView, NodeViewBase sourceView, Vector2 position)
        {
            ((FSMView)_taskView.graphView).TryDisconnectSourceToOriginal(sourceView);

            StateMachine stateMachine = TaskStreamerEditor.Instance.currentGraph as StateMachine;
            Debug.Assert(stateMachine is not null, "stateMachine is null");

            stateMachine.ConnectStates((StateBase)newView.targetNode, (StateBase)sourceView.targetNode);

            _taskView.graphView.TryConnectNodesByEdge(_taskView, newView, sourceView);
        }
    }
}