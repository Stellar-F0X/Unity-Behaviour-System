using System;
using TaskStreamer.Runtime;
using UnityEngine;

namespace TaskStreamer.Tool
{
    internal class NodeGroupFactoryModule<T> : FactoryModule<NodeGroup>
    {
        public NodeGroupFactoryModule(TaskGraphView view, string title, int layer = 1) : base(typeof(T), title, false, layer)
        {
            this._view = view;
        }

        private readonly TaskGraphView _view;


        protected override NodeGroup Create(Type type, Vector2 position, string entryName)
        {
            NodeGroupView groupView = _view.CreateNewNodeGroupView("Node Group", position);
            Debug.Assert(groupView?.groupData is not null, "NodeGroupView's groupData is null");
            return groupView.groupData;
        }
    }
}