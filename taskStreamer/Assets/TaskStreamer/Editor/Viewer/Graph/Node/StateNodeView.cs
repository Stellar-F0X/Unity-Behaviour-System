using System;
using System.Collections.Generic;
using TaskStreamer.FSM;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    internal class StateNodeView : NodeViewBase
    {
        private StateNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml)
        {
            string nodeName = StringUtility.ToNicifyName(targetNode.name, "State");
            base._nodeTypeLabel.text = StringUtility.ToNicifyName(targetNode.GetType().Name, "State");
            base.targetNode.name = nodeName;
            base.title = nodeName;

            this._elementGroup.AddToClassList($"state-node");
            this.Indicator = new StateNodeIndicator(this, TaskStreamerEditor.settings);
            this.Indicator.ApplyBorderColorByState();
        }

        
        
        
        public static StateNodeView Create(NodeBase node, VisualTreeAsset nodeXml)
        {
            StateNodeView nodeView = new StateNodeView(node, nodeXml);
            Debug.Assert(nodeView is not null, "nodeView is null");
            
            nodeView.OnInitialize();
            nodeView.CreatePorts();
            return nodeView;
        }




        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            //remove 'List<Transition>' variable handle
            variableHandles.Remove(v => v.initialValue is List<Transition>);
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

                case StateNodeType.Any:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
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