using System;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public abstract class NodeViewBase : Node
    {
        public NodeViewBase(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this._targetNode = targetNode;
            this.title = targetNode.name;
            this.tooltip = targetNode.tooltip;
            this.viewDataKey = targetNode.guid.ToString();
            this.style.left = targetNode.position.x;
            this.style.top = targetNode.position.y;

            this._elementGroup = this.Q<VisualElement>("group");
            this._nodeBorder = this.Q<VisualElement>("node-border");
            this._nodeTypeLabel = this.Q<TextElement>("node-type-label");
            
            this._connectionEdge = new EdgeDictionary();

            this.Initialize();
            this.CreatePorts();
        }

        public event Action<GraphElement> onNodeSelected;
        public event Action<GraphElement> onNodeUnselected;

        private readonly NodeBase _targetNode;
        
        private readonly VisualElement _nodeBorder;
        private readonly TextElement _nodeTypeLabel;
        
        protected readonly VisualElement _elementGroup;
        
        protected NodeHighlighterBase _highlighter;
        private EdgeDictionary _connectionEdge;

        public Port inputPort;
        public Port outputPort;
        
        
        public VisualElement nodeBorder
        {
            get { return _nodeBorder; }
        }

        internal EdgeDictionary connectionEdge
        {
            get { return _connectionEdge; }
        }

        public NodeBase targetNode
        {
            get { return _targetNode; }
        }
        
        public NodeHighlighterBase highlighter
        {
            get { return _highlighter; }
        }
        
        
        
        private void Initialize()
        {
            _nodeTypeLabel.text = Utility.Utilities.ApplySpacing(_targetNode.GetType().Name);

            if (Application.isPlaying == false)
            {
                SerializedObject serializedNode = new SerializedObject(_targetNode);
                SerializedProperty nameProp = serializedNode.FindProperty("m_Name");
                this.TrackPropertyValue(nameProp, this.ChangeNodeViewName);
            }
        }
        
        
        public override void OnSelected()
        {
            onNodeSelected?.Invoke(this);
        }


        public override void OnUnselected()
        {
            onNodeUnselected?.Invoke(this);
        }
        
        
        //NodeBase CustomEditor에서 그려지는 NodeBase의 Name Field를 수정시, 에디터에서 값 변경을 확인 후, 알림이 전달.
        //등록된 TrackPropertyValue에 등록된 람다가 호출되고 변경된 이름이 property.stringValue로 전돨되며 NodeView의 Title도 변경됨.
        private void ChangeNodeViewName(SerializedProperty nameProperty)
        {
            if (_targetNode is ISubGraphProvider subGraphNode)
            {
                Graph subGraph = TaskStreamerEditor.Instance.graphAsset.GetGraph(subGraphNode.subGraphGuid);
                Debug.Assert(subGraph != null, $"SubGraph {subGraphNode.subGraphGuid} is null");
                subGraph.name = nameProperty.stringValue;
            }

            this.title = nameProperty.stringValue;
        }
        

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(_targetNode, "Behaviour System (Set Position)");

            _targetNode.position.x = Mathf.RoundToInt(newPos.xMin);
            _targetNode.position.y = Mathf.RoundToInt(newPos.yMin);

            EditorUtility.SetDirty(_targetNode);
        }


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
        protected abstract void CreatePorts();


        //상속받은 상위 클래스에서 Disconnect All이라는 ContextualMenu 생성을 방지하기 위해서 오버라이드
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
    }
}