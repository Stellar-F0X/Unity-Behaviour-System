using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary>TaskGraphView 클래스를 정의하고 그래프 작업을 위한 뷰를 제공합니다.</summary>
    [UxmlElement]
    internal partial class TaskGraphView : GraphView
    {
        /// <summary>Task 그래프를 표시하고 편집할 수 있는 그래프 뷰를 제공합니다.</summary>
        public TaskGraphView()
        {
            base.Insert(0, new GridBackground());

            this.AddManipulator(new DoubleClick(0.3f));
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new GraphZoomer(2f, 0.2f));

            styleSheets.Add(TaskStreamerResourceLoader.windowStyle);
        }
        
        /// <summary>내부에서 그래프 뷰 동작을 관리하는 필드입니다.</summary>
        private GraphViewBase _graphView;


        /// <summary>다음 업데이트 시점을 나타내는 변수입니다.</summary>
        private float _nextUpdateTime;



        /// <summary>TaskGraphView에서 사용하는 GraphViewBase 인스턴스를 반환합니다.</summary>
        public GraphViewBase graphView
        {
            get { return _graphView; }
        }


        /// <summary>현재 활성화된 그래프를 반환합니다.</summary>
        public Graph focusGraph
        {
            get { return TaskStreamerEditor.Instance.currentGraph; }
        }



        /// <summary>에디터 뷰를 초기화하고 모든 그래프 요소를 제거합니다.</summary>
        public void ClearEditorView()
        {
            base.graphViewChanged -= this.OnGraphViewChanged;
            this.deleteSelection -= this.OnDeleteSelectionElements;

            base.DeleteElements(base.graphElements);
        }


        /// <summary>그래프 에디터 뷰를 초기화하고 주어진 그래프 데이터를 기반으로 노드, 연결 및 그룹을 재구성합니다.</summary>
        /// <param name="changeGraph">에디터 뷰에 설정할 새로운 그래프 데이터입니다.</param>
        public void TrySetupGraphEditorView(Graph changeGraph)
        {
            Assert.IsNotNull(changeGraph, "change graph is null");
            
            this._graphView = GraphViewBase.CreateGraphView(changeGraph);

            this.ClearEditorView();

            base.graphViewChanged += this.OnGraphViewChanged;
            this.deleteSelection += this.OnDeleteSelectionElements;

            this._graphView.CreateAndConnectNodes(this, this.focusGraph);

            //Recreate node groups when graph view loading
            this.focusGraph.nodeGroup?.ForEach(this.RecreateNodeGroupView);
        }


        /// <summary>입력 포트와 연결 가능한 호환되는 포트들의 목록을 반환합니다.</summary>
        /// <param name="input">연결 검사를 수행할 입력 포트입니다.</param>
        /// <param name="nodeAdapter">포트 연결 규칙을 확인하기 위한 어댑터입니다.</param>
        /// <returns>호환 가능한 출력 포트들의 리스트를 반환합니다.</returns>
        public override List<Port> GetCompatiblePorts(Port input, NodeAdapter nodeAdapter)
        {
            Assert.IsNotNull(input, $"{typeof(TaskGraphView)}: Input is null");

            //direction은 input과 output이므로, 다른 노드라도 같은 포트에 못 꽂게 방지
            return ports.Where(output => input.direction != output.direction && input.node != output.node).ToList();
        }
        


        /// <summary>주어진 노드에 해당하는 NodeView를 찾아 반환합니다.</summary>
        /// <param name="node">대상이 되는 노드 객체입니다.</param>
        /// <returns>노드에 해당하는 NodeView를 반환하며, 노드를 찾을 수 없을 경우 null을 반환합니다.</returns>
        public NodeViewBase FindNodeView(NodeBase node)
        {
            if (node is null || node.guid.IsEmpty())
            {
                return null;
            }

            return this.GetNodeByGuid(node.guid.ToString()) as NodeViewBase;
        }


        /// <summary>주어진 GUID를 기준으로 해당 NodeView를 찾습니다.</summary>
        /// <param name="guid">찾고자 하는 노드의 고유 식별자입니다.</param>
        /// <returns>해당 GUID에 일치하는 NodeViewBase 인스턴스입니다. 없으면 null을 반환합니다.</returns>
        public NodeViewBase FindNodeView(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            return this.GetNodeByGuid(guid) as NodeViewBase;
        }


        /// <summary>런타임 중 노드 뷰들을 업데이트합니다.</summary>
        public void UpdateNodeView()
        {
            if (Time.unscaledTime < _nextUpdateTime)
            {
                return;
            }

            float interval = TaskStreamerEditor.settings.updateInterval;
            this._nextUpdateTime = Time.unscaledTime + interval;

            foreach (Node node in this.nodes)
            {
                ((NodeViewBase)node).indicator.TryHighlight(interval);
            }
        }


#region Mouse Related Events

        /// <summary>마우스 위치에서 컨텍스트 메뉴(노드 생성) 창을 엽니다.</summary>
        /// <param name="mousePosition">컨텍스트 메뉴 창을 열 좌표입니다.</param>
        /// <param name="onNodeCreated">노드 생성 후 실행할 콜백입니다.</param>
        public void OpenContextualMenuWindow(Vector2 mousePosition, Action<NodeViewBase> onNodeCreated = null)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            Assert.IsNotNull(this._graphView, "Task streamer graph view is null");

            BindingWindow bindingWindow = _graphView.CreateGraphNodeCreationWindow(this);
            bindingWindow.RegisterCreationCallbackOnce(onNodeCreated);
            bindingWindow.OpenWindow(mousePosition);
        }


        /// <summary>우클릭 시 컨텍스트 메뉴를 구성합니다.</summary>
        /// <param name="evt">컨텍스트 메뉴 이벤트 데이터입니다.</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            this.OpenContextualMenuWindow(evt.mousePosition);
        }


        /// <summary>지정된 노드를 선택합니다.</summary>
        /// <param name="nodeView">선택할 노드를 나타내는 NodeViewBase 인스턴스</param>
        public void SelectNodeForCode(NodeViewBase nodeView)
        {
            Assert.IsNotNull(nodeView?.targetNode, "node view or target node is null");

            if (selection.Count > 0)
            {
                base.ClearSelection();
            }

            base.AddToSelection(nodeView);
        }


        public void CallSelectionEvent(ISelectableView selectedView)
        {
            if (TaskStreamerEditor.isLoadingTreeToView)
            {
                return;
            }

            FloatingInspector inspector = TaskStreamerEditor.Instance.inspectorView;
            Assert.IsNotNull(inspector, $"floating inspector is null reference");

            GraphElement graphElement = selectedView as GraphElement;
            Assert.IsNotNull(graphElement, $"Selected element is not {typeof(GraphElement)}");

            inspector.UpdateSelection(graphElement);
        }


        /// <summary> 선택 해제가 될 때, 실행시킬 기능이 없지만 형식상 구현해둔 함수입니다. </summary>
        public void CallUnselectionEvent(ISelectableView selectedView) { }

#endregion


#region Delete Of Modify Graph Elements

        /// <summary>그래프 뷰가 변경될 때 호출되는 콜백 메서드입니다.</summary>
        /// <param name="graphViewChange">그래프 뷰의 변경 사항을 담고 있는 객체입니다.</param>
        /// <returns>처리된 그래프 뷰 변경 사항 객체를 반환합니다.</returns>
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove is not null)
            {
                //에디터 뷰에서 삭제된 그래프 뷰 요소를 순회하며 대응되는 노드나 간선, 그룹 등의 데이터를 제거한다.
                foreach (GraphElement element in graphViewChange.elementsToRemove)
                {
                    switch (element)
                    {
                        case Edge edge: this.graphView.DisconnectNodesByEdge(focusGraph, edge); break;

                        case NodeViewBase nodeView: this.graphView.DeleteNodeFromGraph(focusGraph, nodeView); break;

                        case NodeGroupView groupView: this.focusGraph.DeleteGroupDataAndRemoveFromList(groupView.groupData); break;
                    }
                }
            }

            //노드가 생성되거나 이동된 경우, 노드의 위치를 업데이트하고 새롭게 생성된 간선을 연결한다.
            if (graphViewChange.edgesToCreate is not null)
            {
                _graphView.ConnectNodesByEdges(this, focusGraph, graphViewChange.edgesToCreate);
            }

            //노드의 위치를 업데이트된 경우, BT는 앞의 자식을 먼저 순회하기 때문에 X좌표에 따른 순서를 정렬하여 갱신해준다. 
            if (graphViewChange.movedElements is not null)
            {
                _graphView.NotifyNodePositionChanged(this, graphViewChange.movedElements);
            }

            return graphViewChange;
        }


        /// <summary>선택된 요소들을 삭제할 때 호출되는 콜백 메서드입니다.</summary>
        /// <param name="operationName">현재 수행 중인 작업의 이름입니다.</param>
        /// <param name="user">사용자 입력을 나타내는 객체입니다.</param>
        private void OnDeleteSelectionElements(string operationName, AskUser user)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            this._graphView.FilterSelectionElements(this.selection);

            //DeleteSelection는 내부적으로 Selection 배열을 이용해서 VisualElement들을 제거함.
            //따라서 삭제되면 안되는 요소들만 Selection 배열에서 제거한 뒤, 현재 선택된 요소들(Selection 배열)을 제거하면 됨.
            this.DeleteSelection();
        }

#endregion


#region Create Graph Elements

        /// <summary> GraphView가 로드될 때, Node Group Data를 토대로 Node Group View를 다시 생성합니다. </summary>
        /// <param name="data"> Node Group View를 생성하기 위한 Node Group Data 입니다. </param>
        private void RecreateNodeGroupView(NodeGroup data)
        {
            NodeGroupView nodeGroupView = new NodeGroupView(data, TaskStreamerEditor.settings.nodeGroupColor);
            nodeGroupView.AddElements(nodes.Where(n => n is NodeViewBase v && data.Contains(v.targetNode.guid)));
            nodeGroupView.SetPosition(new Rect(data.position, Vector2.zero));
            base.AddElement(nodeGroupView);
        }
        
        
        /// <summary>지정된 타입과 위치에서 새로운 노드를 생성하고 NodeView를 반환합니다.</summary>
        /// <param name="type">생성할 노드의 타입입니다.</param>
        /// <param name="mousePosition">노드의 초기 위치입니다.</param>
        /// <returns>생성된 노드와 연결된 NodeViewBase 인스턴스입니다.</returns>
        public NodeViewBase CreateNewNodeAndView(Type type, Vector2 mousePosition)
        {
            NodeBase node = focusGraph.CreateAndAddNodeToList(type.Name, type);
            node.position = Vector2Int.CeilToInt(mousePosition);

            NodeViewBase nodeView = this._graphView.RecreateNodeViewOnLoad(node);
            this.AddNewNodeView(nodeView);
            return nodeView;
        }


        /// <summary>새로운 노드 뷰를 추가하고 관련 이벤트를 처리합니다.</summary>
        /// <param name="nodeView">추가할 노드 뷰 객체입니다.</param>
        public void AddNewNodeView(NodeViewBase nodeView)
        {
            Assert.IsNotNull(nodeView, "node view is null");
            Assert.IsNotNull(nodeView.targetNode, "target node is null");

            this.AddElement(nodeView);
        }


        /// <summary>새로운 노드 그룹 뷰를 생성하고 반환합니다.</summary>
        /// <param name="title">노드 그룹의 제목.</param>
        /// <param name="position">노드 그룹의 초기 위치.</param>
        /// <returns>생성된 NodeGroupView 객체.</returns>
        public NodeGroupView CreateNewNodeGroupView(string title, Vector2 position)
        {
            NodeGroup nodeNodeGroupData = focusGraph.CreateGroupDataAndAddToList(title, position);
            NodeGroupView groupView = new NodeGroupView(nodeNodeGroupData, TaskStreamerEditor.settings.nodeGroupColor);

            groupView.SetPosition(new Rect(position, Vector2.zero));

            base.AddElement(groupView);
            return groupView;
        }

#endregion
    }
}