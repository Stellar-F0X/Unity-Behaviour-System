using System;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class StateNodeView : NodeView
    {
        public StateNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml) { }
        

        public EdgeDictionary connectionEdge
        {
            get { return _connectionEdge; }
        }
        

        protected override void Initialize()
        {
            _elementGroup.AddToClassList($"state-node-{((StateBase)targetNode).nodeType}");
            base.Initialize();
        }

        
        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            return new PortView(EGraphType.FSM, direction, capacity);
        }
        

        protected override void CreatePorts()
        {
            switch (((StateBase)targetNode).nodeType)
            {
                case EStateNodeType.Enter:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }
                
                case EStateNodeType.Exit:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                    break;
                }

                case EStateNodeType.SubGraph:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }
                
                case EStateNodeType.Any:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }
                
                case EStateNodeType.Action:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }
            }
            
            this.SetupPort(inputPort, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetupPort(outputPort, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }
        
        
        public override void SetEdgeColor(EdgeDictionary control, Color color)
        {
            
        }
    }
}