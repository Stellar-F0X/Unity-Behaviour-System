using UnityEngine;

namespace TaskStreamer.Tool
{
    public class FSMEdgeConnectorListener : GraphEdgeConnectorListener
    {
        protected override void CreateAndLinkFromOriginalToNewNode(NodeViewBase sourceNodeView, NodeViewBase targetNodeView, Vector2 position)
        {
            GraphViewControl control = TaskStreamerEditor.Instance.view.graphViewControl;
            
            control.TryDisconnectParentToChild(sourceNodeView);
        }
        
        
        protected override void CreateAndLinkFromNewToOriginalNode(NodeViewBase sourceNodeView, NodeViewBase targetNodeView, Vector2 position)
        {
            GraphViewControl control = TaskStreamerEditor.Instance.view.graphViewControl;
            
            control.TryDisconnectChildToParent(targetNodeView);
        }
    }
}