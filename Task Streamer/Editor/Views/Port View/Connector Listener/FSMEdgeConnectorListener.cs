using UnityEngine;

namespace TaskStreamer.Tool
{
    public class FSMEdgeConnectorListener : GraphEdgeConnectorListener
    {
        protected override void CreateAndLinkFromOriginalToNewNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position)
        {
            GraphViewProcessor processor = TaskStreamerEditor.Instance.view.graphViewProcessor;
            
            processor.TryDisconnectParentToChild(sourceNodeView);
        }
        
        
        protected override void CreateAndLinkFromNewToOriginalNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position)
        {
            GraphViewProcessor processor = TaskStreamerEditor.Instance.view.graphViewProcessor;
            
            processor.TryDisconnectChildToParent(targetNodeView);
        }
    }
}