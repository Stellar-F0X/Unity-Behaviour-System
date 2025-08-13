using System;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class NodeGroupFactoryModule : FactoryModule<NodeGroup>
    {
        public NodeGroupFactoryModule(TaskGraphView view, Type targetType, string title, int layer = 1) : base(targetType, title, false, false, layer)
        {
            this._view = view;
        }

        private readonly TaskGraphView _view;


        protected override NodeGroup Create(Type type, Vector2 position)
        {
            NodeGroupView groupView = _view.CreateNewNodeGroupView("Node Group", position);
            Debug.Assert(groupView?.groupData is not null, "NodeGroupView's groupData is null");
            return groupView.groupData;
        }
    }
}