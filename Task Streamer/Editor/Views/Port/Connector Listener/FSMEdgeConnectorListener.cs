using UnityEngine;

namespace TaskStreamer.Tool
{
    public class FSMEdgeConnectorListener : GraphEdgeConnectorListener
    {
        protected override void CreateAndLinkFromOriginalToNewNode(NodeViewBase sourceNodeView, NodeViewBase targetNodeView, Vector2 position)
        {
            GraphViewBase control = TaskStreamerEditor.Instance.view.graphView;
            
            control.TryDisconnectParentToChild(sourceNodeView);
        }
        
        
        protected override void CreateAndLinkFromNewToOriginalNode(NodeViewBase sourceNodeView, NodeViewBase targetNodeView, Vector2 position)
        {
            GraphViewBase control = TaskStreamerEditor.Instance.view.graphView;
            
            control.TryDisconnectChildToParent(targetNodeView);
        }
    }
}