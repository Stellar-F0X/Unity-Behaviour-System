using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using TypeUtility = TaskStreamer.Utility.TypeUtility;

namespace TaskStreamer.Tool
{
    /// Node 클래스에서 확장된 추상 베이스 클래스이며, 노드의 시각적 표현과 데이터 동기화를 관리합니다.
    public abstract class NodeViewBase : Node
    {
        protected NodeViewBase(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this._elementGroup = this.Q<VisualElement>("contents");
            this._nodeBorder = this.Q<VisualElement>("node-border");
            this._nodeTypeLabel = this.Q<TextElement>("type-label");
            
            //title은 sub class에서 처리.
            this.viewDataKey = targetNode.guid.ToString();
            this._targetNode = targetNode;
            this.tooltip = targetNode.tooltip;
            this.style.left = targetNode.position.x;
            this.style.top = targetNode.position.y;
        }

        
        
        /// 사용자가 Node를 선택했을 때 발생하는 이벤트로, 선택된 Node 정보를 전달합니다.
        public event Action<GraphElement> onNodeSelected;

        
        /// onNodeUnselected 변수는 노드가 선택 해제될 때 실행되는 이벤트를 정의합니다.
        public event Action<GraphElement> onNodeUnselected;
        
        
        /// 노드의 종류를 표시하기 위한 UI 텍스트 요소를 나타내는 변수입니다.
        protected readonly TextElement _nodeTypeLabel;

        
        /// <summary> 노드 내부에서 UI 그룹 요소를 참조하기 위한 VisualElement 변수. </summary>
        protected readonly VisualElement _elementGroup;

        
        /// NodeViewBase 클래스 내에서 노드의 테두리 스타일링 및 레이아웃을 정의하는 비주얼 엘리먼트입니다.
        private readonly VisualElement _nodeBorder;

        
        /// <summary> 노드의 상태 또는 강조 표시를 나타내며, UI 테두리 색상 등의 시각적 피드백을 처리하는 데 사용됩니다. </summary>
        protected NodeIndicatorBase Indicator;

        
        /// _connectionEdges 변수는 각 노드의 연결 정보를 관리하는 Dictionary로, UGUID와 Edge 객체의 매핑을 저장합니다.
        private Dictionary<UGUID, Edge> _connectionEdges = new Dictionary<UGUID, Edge>();

        
        /// _targetNode는 현재 NodeView가 참조하는 데이터 모델인 NodeBase 타입의 객체로, 노드의 상태와 정보를 관리합니다.
        private NodeBase _targetNode;

        
        /// 노드의 입력 데이터를 처리하기 위한 입력 포트이며, 다른 노드와 연결될 수 있습니다.
        public Port inputPort;

        
        /// outputPort는 노드의 출력 데이터를 다른 노드와 연결할 수 있도록 제공하는 출력 포트입니다.
        public Port outputPort;


        /// <summary>
        /// Node 간 연결을 나타내는 Edge를 UGUID 키로 관리하는 Dictionary 속성입니다.
        /// 연결된 Edge를 조회, 추가, 또는 조작할 수 있습니다.
        /// </summary>
        internal Dictionary<UGUID, Edge> connectionEdges
        {
            get { return _connectionEdges; }
        }
        

        /// 특정 노드 또는 엣지의 필드 속성 정보를 저장하는 읽기 전용 List 객체입니다.
        /// TypeUtility를 통해 초기화되며, 필드 속성 관리를 위한 데이터로 활용됩니다.
        internal List<VariableHandle> variableHandles
        {
            get;
            private set;
        }

        
        /// <summary>
        /// 노드 이름 변경 시 실행되는 콜백 이벤트입니다.
        /// 콜백 함수에 새 이름을 전달하여 노드의 이름을 업데이트합니다.
        /// </summary>
        internal Action<string> onRenamingNode
        {
            get;
            private set;
        }

        
        /// 노드의 테두리를 나타내는 UI 요소를 반환하는 속성입니다.
        /// 노드 스타일 및 상태에 따라 동적으로 테두리 색상이 변경됩니다.
        public VisualElement nodeBorder
        {
            get { return _nodeBorder; }
        }
        

        /// 연결된 데이터를 나타내는 NodeBase의 인스턴스를 반환합니다.
        /// 노드의 로직 또는 정보를 참조 및 수정하는 데 사용됩니다.
        public NodeBase targetNode
        {
            get { return _targetNode; }
        }
        

        /// <summary>
        /// NodeViewBase 클래스에서 노드 상태를 기반으로 하이라이트 효과와 테두리 색상을 관리하는 NodeIndicatorBase 타입의 Indicator 속성입니다.
        /// </summary>
        public NodeIndicatorBase indicator
        {
            get { return Indicator; }
        }
        


        /// <summary> NodeView 를 초기화하고, 관련 속성 및 필드 정보를 설정합니다. </summary>
        protected virtual void OnInitialize()
        {
            this.onRenamingNode = this.ChangeNodeViewName;
            
            this.variableHandles = TypeUtility.TryGetFieldHandles(targetNode.GetType(), this.targetNode);
            
            Debug.Assert(this.variableHandles != null, $"Properties is null. Type: {targetNode.GetType().FullName}");
        }


        
        /// 노드가 선택되었을 때 호출되는 메서드입니다.
        /// 노드 선택 이벤트를 트리거하며, 데이터 로딩 중에는 동작하지 않습니다.
        public override void OnSelected()
        {
            if (TaskStreamerEditor.isLoadingTreeToView)
            {
                return;
            }
            
            onNodeSelected?.Invoke(this);
        }

        

        /// 노드가 선택 해제될 때 호출되며, 선택 해제 이벤트를 트리거합니다.
        /// TaskStreamerEditor에서 로드 중에는 실행되지 않습니다.
        public override void OnUnselected()
        {
            if (TaskStreamerEditor.isLoadingTreeToView)
            {
                return;
            }
            
            onNodeUnselected?.Invoke(this);
        }


        
        /// <summary> NodeBase의 Name 변경 이벤트 호출 시, 해당 NodeView의 제목을 수정하고 관련 SubGraph의 이름도 업데이트합니다. </summary>
        /// <param name="newName">변경할 새로운 노드 이름</param>
        private void ChangeNodeViewName(string newName)
        {
            if (_targetNode is ISubGraphProvider subGraphNode)
            {
                Graph subGraph = TaskStreamerEditor.Instance.graphAsset.GetGraph(subGraphNode.subGraphGuid);
                Debug.Assert(subGraph != null, $"SubGraph {subGraphNode.subGraphGuid} is null");
                subGraph.name = newName;
            }

            this.title = newName;
            this.targetNode.name = newName;
        }

        

        /// <summary> 노드의 위치를 설정합니다. </summary>
        /// <param name="newPos">노드의 새 위치를 나타내는 Rect 객체입니다.</param>
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "Behaviour System (Set Position)");

            _targetNode.position.x = Mathf.RoundToInt(newPos.xMin);
            _targetNode.position.y = Mathf.RoundToInt(newPos.yMin);

            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
        }


        
        /// <summary> 지정된 Port를 설정하고, 방향과 이름을 할당한 후, 컨테이너에 추가합니다. </summary>
        /// <param name="port">설정할 Port 객체입니다.</param>
        /// <param name="portName">포트에 설정할 이름입니다.</param>
        /// <param name="direction">Port의 Flex Direction 설정입니다.</param>
        /// <param name="container">Port를 추가할 대상 컨테이너입니다.</param>
        protected void SetPort(Port port, string portName, FlexDirection direction, VisualElement container)
        {
            if (port is null)
            {
                return;
            }

            port.pickingMode = TaskStreamerEditor.canEditGraph ? PickingMode.Position : PickingMode.Ignore;
            port.style.flexDirection = direction;
            port.portName = portName;
            container.Add(port);
        }
        
        

        //NodeView에 포트를 생성합니다.
        /// NodeView에 필요한 입력 및 출력 포트를 생성하는 메서드입니다.
        /// 서브 클래스에서 노드 유형에 따라 구현됩니다.
        protected abstract void CreatePorts();


        
        //상속받은 상위 클래스에서 Disconnect All이라는 ContextualMenu 생성을 방지하기 위해서 오버라이드
        /// <param name="evt">컨텍스트 메뉴를 초기화하고 적용하는 이벤트입니다.</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
    }
}