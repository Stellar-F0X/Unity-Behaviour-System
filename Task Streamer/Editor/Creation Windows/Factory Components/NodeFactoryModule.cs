using System;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class NodeFactoryModule : FactoryModule<NodeViewBase>
    {
        public NodeFactoryModule(TaskGraphView view, Type targetType, string title, int layer = 1) : base(targetType, title, true, true, layer)
        {
            this._view = view;
        }

        private readonly TaskGraphView _view;


        protected override NodeViewBase Create(Type type, Vector2 position)
        {
            return _view.CreateNewNodeAndView(type, position);
        }


        protected override void AfterCreate(NodeViewBase creation)
        {
            if (creation.targetNode is ISubGraphProvider graphNode)
            {
                GraphAsset graphAsset = TaskStreamerEditor.Instance.graphAsset;
                Debug.Assert(graphAsset is not null, $"{nameof(NodeFactoryModule)}: GraphAsset is null");

                Graph baseGraph = TaskStreamerEditor.Instance.view.focusGraph;
                Graph newGraph = null;
                
                Utilities.CreateGraph(graphAsset, graphNode.subGraphType, ref newGraph, creation.title);

                baseGraph.AddSubGraph(newGraph);
                graphNode.subGraphGuid = newGraph.guid;
                EditorUtility.SetDirty(graphNode as NodeBase);
            }

            _view.SelectNode(creation);
        }
    }
}