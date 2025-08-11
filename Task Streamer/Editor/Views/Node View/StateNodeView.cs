using System;
using TaskStreamer.FSM;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class StateNodeView : NodeViewBase
    {
        public StateNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml)
        {
            _elementGroup.AddToClassList($"state-node-{((StateBase)targetNode).nodeType}");
            
            _highlighter = new StateNodeHighlighter(this, TaskStreamerEditor.settings);
        }
        


        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return new PortView(GraphType.FSM, direction, capacity);
        }


        protected override void CreatePorts()
        {
            switch (((StateBase)targetNode).nodeType)
            {
                case StateNodeType.Enter:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case StateNodeType.Exit:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                    break;
                }

                case StateNodeType.SubGraph:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }

                case StateNodeType.Any:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }

                case StateNodeType.Action:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }
            }

            this.SetPort(inputPort, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetPort(outputPort, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }
    }
}