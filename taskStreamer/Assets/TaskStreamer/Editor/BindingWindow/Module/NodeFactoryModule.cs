using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    internal class NodeFactoryModule<T> : FactoryModule<NodeViewBase>
    {
        public NodeFactoryModule(TaskGraphView view, string title, int layer = 1) : base(typeof(T), title, true, layer)
        {
            this._view = view;
        }


        private readonly TaskGraphView _view;


        protected override NodeViewBase Create(Type type, Vector2 position, string entryName)
        {
            return _view.CreateNewNodeAndView(type, position);
        }


        protected override void AfterCreate(NodeViewBase creation)
        {
            if (creation.targetNode is ISubGraphProvider graphNode)
            {
                GraphAsset graphAsset = TaskStreamerEditor.Instance.graphAsset;
                Debug.Assert(graphAsset is not null, $"{nameof(NodeFactoryModule<T>)}: GraphAsset is null");

                Graph baseGraph = TaskStreamerEditor.Instance.taskGraphView.focusGraph;
                Graph newGraph = ObjectFactory.CreateGraph(graphAsset, graphNode.subGraphType, creation.title);

                baseGraph.AddSubGraph(newGraph);
                graphNode.subGraphGuid = newGraph.guid;
                UnityEditor.EditorUtility.SetDirty(graphAsset);
            }

            _view.SelectNodeForCode(creation);
        }
    }
}