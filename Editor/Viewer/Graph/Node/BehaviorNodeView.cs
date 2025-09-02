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
        public BehaviorNodeView(NodeBase targetNode, VisualTreeAsset nodeUxml) : base(targetNode, nodeUxml)
        {
            string nodeName = StringUtility.ToNicifyName(targetNode.name, "Node");
            base._nodeTypeLabel.text = nodeName;
            base.targetNode.name = nodeName;
            base.title = nodeName;
            
            this._serviceContainer = this.Q<VisualElement>("service-container");
            this._elementGroup.AddToClassList($"behavior-node");

            this.Indicator = new BehaviorIndicator(this, TaskStreamerEditor.settings);
            _variableHandlesDic = new ObservableDictionary<ServiceBase, List<VariableHandle>>();
            this.Indicator.ApplyBorderColorByState();

            this.GetServiceVariableHandleList();
        }


        private readonly ObservableDictionary<ServiceBase, List<VariableHandle>> _variableHandlesDic;

        private readonly VisualElement _serviceContainer;
        
        private List<ServiceBase> _serviceList;



        internal ObservableDictionary<ServiceBase, List<VariableHandle>> variableHandlesDic
        {
            get { return _variableHandlesDic; }
        }



        private void GetServiceVariableHandleList()
        {
            VariableHandle handle = base.variableHandles.Find(v => v.initialValue is List<ServiceBase>);
            Debug.Assert(handle is not null, $"{typeof(BehaviorNodeView)}: Failed to find service handle in variable handles");
            
            
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
                    _serviceContainer.Clear();
                    _serviceList.Clear();
                    break;
                }
                
                case NotifyCollectionChangedAction.Add:
                {
                    _serviceContainer.Add(new ServiceView(service));
                    _serviceList.Add(service);
                    break;
                } 

                case NotifyCollectionChangedAction.Remove:
                {
                    int index = _serviceList.IndexOf(service);
                    _serviceContainer.RemoveAt(index);
                    _serviceList.RemoveAt(index);
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