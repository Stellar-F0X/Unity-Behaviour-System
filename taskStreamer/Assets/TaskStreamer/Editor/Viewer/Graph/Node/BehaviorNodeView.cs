using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    internal class BehaviorNodeView : NodeViewBase
    {
        private BehaviorNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml)
        {
            string nodeName = StringUtility.ToNicifyName(targetNode.name, "Node");
            base._nodeTypeLabel.text = StringUtility.ToNicifyName(targetNode.GetType().Name, "Node");
            base.targetNode.name = nodeName;
            base.title = nodeName;
            
            this._serviceContainerView = this.Q<VisualElement>("service-container");
            this._elementGroup.AddToClassList($"behavior-node");

            this.Indicator = new BehaviorIndicator(this, TaskStreamerEditor.settings);
            
            this.serviceList.ForEach(s => this._serviceContainerView.Add(new ServiceView(s)));
            this.serviceListChangedAction += this.OnServiceViewListChanged;
        }


        //TODO: 추후 BehaviorNodeBase의 List<ServiceBase> 자체를 ObservableList<ServiceBase>로 변경하고 해당 이벤트를 삭제.
        internal readonly Action<NotifyListChanged, ServiceBase> serviceListChangedAction;
        
        
        private readonly VisualElement _serviceContainerView;

        
        
        internal List<ServiceBase> serviceList
        {
            get { return ((BehaviorNodeBase)targetNode).services; }
        }
        


        //TODO: Unity가 C# 11을 지원하면 Static Abstract Interface로 StateNodeView와 함께 팩토리 함수를 묶자.
        public static BehaviorNodeView Create(NodeBase node, VisualTreeAsset nodeXml)
        {
            BehaviorNodeView nodeView = new BehaviorNodeView(node, nodeXml);
            Debug.Assert(nodeView is not null, "nodeView is null");
            
            nodeView.OnInitialize();
            nodeView.CreatePorts();
            return nodeView;
        }



        private void OnServiceViewListChanged(NotifyListChanged action, ServiceBase service)
        {
            switch (action)
            {
                case NotifyListChanged.Add:
                {
                    this.serviceList.Add(service);
                    this._serviceContainerView.Add(new ServiceView(service));
                    break;
                } 

                case NotifyListChanged.Remove:
                {
                    int index = serviceList.IndexOf(service);
                    this.serviceList.RemoveAt(index);
                    this._serviceContainerView.RemoveAt(index);
                    break;
                } 
            }
        }



        public void SortChildren()
        {
            if (((BehaviorNodeBase)targetNode).nodeType != BehaviorNodeType.Composite)
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
            return new PortView(GraphType.BT, direction, capacity);
        }

        

        protected override void CreatePorts()
        {
            switch (((BehaviorNodeBase)targetNode).nodeType)
            {
                case BehaviorNodeType.Root:
                {
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case BehaviorNodeType.Action:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case BehaviorNodeType.SubGraph:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
                }

                case BehaviorNodeType.Composite:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;
                }

                case BehaviorNodeType.Decorator:
                {
                    inputPort = this.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    outputPort = this.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
                }
            }

            this.SetPort(inputPort, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetPort(outputPort, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }
    }
}