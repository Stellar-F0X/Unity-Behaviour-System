using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using TypeUtility = TaskStreamer.Utility.TypeUtility;

namespace TaskStreamer.Tool
{
    public abstract class NodeViewBase : Node
    {
        public NodeViewBase(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this._elementGroup = this.Q<VisualElement>("group");
            this._nodeBorder = this.Q<VisualElement>("node-border");
            this._nodeTypeLabel = this.Q<TextElement>("node-type-label");
            
            this.title = StringUtility.ApplySpacing(targetNode.name);
            this.viewDataKey = targetNode.guid.ToString();
            this._targetNode = targetNode;
            this.tooltip = targetNode.tooltip;
            this.style.left = targetNode.position.x;
            this.style.top = targetNode.position.y;
            
            this.Initialize();
            this.CreatePorts();
        }

        public event Action<GraphElement> onNodeSelected;
        public event Action<GraphElement> onNodeUnselected;

        protected readonly TextElement _nodeTypeLabel;
        protected readonly VisualElement _elementGroup;
        private readonly VisualElement _nodeBorder;

        protected NodeIndicatorBase Indicator;
        
        private readonly EdgeDictionary _connectionEdges = new EdgeDictionary();
        private NodeBase _targetNode;

        public Port inputPort;
        public Port outputPort;


        public VisualElement nodeBorder
        {
            get { return _nodeBorder; }
        }

        internal EdgeDictionary connectionEdges
        {
            get { return _connectionEdges; }
        }

        internal List<object> fieldProperties
        {
            get;
            private set;
        }

        internal Action<string> onRenamingNode
        {
            get;
            private set;
        }

        public NodeBase targetNode
        {
            get { return _targetNode; }
        }

        public NodeIndicatorBase indicator
        {
            get { return Indicator; }
        }



        private void Initialize()
        {
            this.onRenamingNode = this.ChangeNodeViewName;
            
            this.fieldProperties = TypeUtility.TryGetFieldProperties(targetNode.GetType(), this.targetNode);
            
            Debug.Assert(this.fieldProperties != null, $"Properties is null. Type: {targetNode.GetType().FullName}");
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
        private void ChangeNodeViewName(string newName)
        {
            if (_targetNode is ISubGraphProvider subGraphNode)
            {
                Graph subGraph = TaskStreamerEditor.Instance.graphAsset.GetGraph(subGraphNode.subGraphGuid);
                Debug.Assert(subGraph != null, $"SubGraph {subGraphNode.subGraphGuid} is null");
                subGraph.name = newName;
            }

            this.title = newName;
        }


        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "Behaviour System (Set Position)");

            _targetNode.position.x = Mathf.RoundToInt(newPos.xMin);
            _targetNode.position.y = Mathf.RoundToInt(newPos.yMin);

            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
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