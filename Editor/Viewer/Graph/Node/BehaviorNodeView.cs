using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using TaskStreamer.BT;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class BehaviorNodeView : NodeViewBase
    {
        private BehaviorNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml)
        {
            string nodeName = StringUtility.ToNicifyName(targetNode.name, "Node");
            base._nodeTypeLabel.text = nodeName;
            base.targetNode.name = nodeName;
            base.title = nodeName;
            
            this._serviceContainer = this.Q<VisualElement>("service-container");
            this._elementGroup.AddToClassList($"behavior-node");

            this.Indicator = new BehaviorIndicator(this, TaskStreamerEditor.settings);
            _variableHandlesDic = new ObservableDictionary<ServiceBase, List<VariableHandle>>();
        }
        

        private readonly ObservableDictionary<ServiceBase, List<VariableHandle>> _variableHandlesDic;

        private readonly VisualElement _serviceContainer;
        
        private List<ServiceBase> _serviceList;




        //serviceBase 객체의 필드를 variableHandle로 미리 캐싱해둠. 
        internal ObservableDictionary<ServiceBase, List<VariableHandle>> variableHandlesDic
        {
            get { return _variableHandlesDic; }
        }
        
        
        internal List<ServiceBase> serviceList
        {
            get { return _serviceList; }
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
        
        
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            VariableHandle handle = base.variableHandles[0];
            Debug.Assert(handle is not null, $"{typeof(BehaviorNodeView)}: Failed to find service handle in variable handles");
            variableHandles.RemoveAt(0);
            
            
            this._serviceList = handle.GetValue<List<ServiceBase>>();
            Debug.Assert(this._serviceList is not null, $"{typeof(BehaviorNodeView)}: Service list is null in variable handle");

            
            this._variableHandlesDic.onCollectionItemChanged -= this.OnServiceViewListChanged;
            
            foreach (ServiceBase service in _serviceList)
            {
                this._variableHandlesDic.Add(service, TypeUtility.TryGetFieldHandles(service.GetType(), service));
                this._serviceContainer.Add(new ServiceView(service));
            }
            
            this._variableHandlesDic.onCollectionItemChanged += this.OnServiceViewListChanged;
        }



        private void OnServiceViewListChanged(Dictionary<ServiceBase, List<VariableHandle>> _, NotifyCollectionChangedAction action, ServiceBase service, List<VariableHandle> handleList)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Reset:
                {
                    this._serviceContainer.Clear();
                    this._serviceList.Clear();
                    break;
                }
                
                case NotifyCollectionChangedAction.Add:
                {
                    this._serviceContainer.Add(new ServiceView(service));
                    this._serviceList.Add(service);
                    break;
                } 

                case NotifyCollectionChangedAction.Remove:
                {
                    int index = _serviceList.IndexOf(service);
                    this._serviceContainer.RemoveAt(index);
                    this._serviceList.RemoveAt(index);
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