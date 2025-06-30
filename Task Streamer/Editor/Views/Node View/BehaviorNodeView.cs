using System;
using TaskStreamer.BT;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class BehaviorNodeView : NodeView
    {
        public BehaviorNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml) { }



        public Edge parentConnectionEdge
        {
            get { return _connectionEdge[UGUID.Empty]; }

            set { _connectionEdge[UGUID.Empty] = value; }
        }
        

        
        protected override void Initialize()
        {
            _elementGroup.AddToClassList($"behaviour-node-{((BehaviorNodeBase)targetNode).nodeType}");
            base.Initialize();
        }


        public void SortChildren()
        {
            if (((BehaviorNodeBase)targetNode).nodeType != EBehaviorNodeType.Composite)
            {
                return;
            }

            if (targetNode is CompositeNode compositeNode)
            {
                compositeNode.children.Sort((l, r) => l.position.x < r.position.x ? -1 : 1);
            }
        }


        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return new PortView(EGraphType.BT, direction, capacity);
        }


        protected override void CreatePorts()
        {
            switch (((BehaviorNodeBase)targetNode).nodeType)
            {
                case EBehaviorNodeType.Root:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case EBehaviorNodeType.Action:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case EBehaviorNodeType.SubGraph:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case EBehaviorNodeType.Composite:
                {
                    inputPort =  this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }

                case EBehaviorNodeType.Decorator:
                {
                    inputPort =  this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }
            }

            this.SetupPort(inputPort, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetupPort(outputPort, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }

        
        public override void SetEdgeColor(EdgeDictionary control, Color color)
        {
            
        }


        protected override void SetBorderColorByStatus()
        {
            switch (((BehaviorNodeBase)targetNode).status)
            {
                case EStatus.Failure: base.SetBorderColor(_nodeBorder.style, TaskStreamerEditor.settings.nodeFailureColor); break;

                case EStatus.Success: base.SetBorderColor(_nodeBorder.style, TaskStreamerEditor.settings.nodeSuccessColor); break;
            }
        }
    }
}