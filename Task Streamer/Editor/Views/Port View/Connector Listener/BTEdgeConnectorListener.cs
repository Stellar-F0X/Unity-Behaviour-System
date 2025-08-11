using TaskStreamer.BT;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class BTEdgeConnectorListener : GraphEdgeConnectorListener
    {
        public BTEdgeConnectorListener()
        {
            _control = TaskStreamerEditor.Instance.view.graphViewControl;
            _graphView = TaskStreamerEditor.Instance.view;
        }
        
        private readonly GraphViewControl _control;
        private readonly TaskGraphView _graphView;
        
        
        protected override void CreateAndLinkFromOriginalToNewNode(NodeViewBase sourceNodeView, NodeViewBase targetNodeView, Vector2 position)
        {
            _control.TryDisconnectParentToChild(sourceNodeView);

            if (TaskStreamerEditor.Instance.graphAsset.main is not BehaviorTree)
            {
                return;
            }

            if (_control.TryConnectNodesByEdge(_graphView, sourceNodeView, targetNodeView))
            {
                BehaviorTree.ConnectNodes((BehaviorNodeBase)sourceNodeView.targetNode, (BehaviorNodeBase)targetNodeView.targetNode);
            }
        }


        protected override void CreateAndLinkFromNewToOriginalNode(NodeViewBase sourceNodeView, NodeViewBase targetNodeView, Vector2 position)
        {
            _control.TryDisconnectChildToParent(targetNodeView);

            if (TaskStreamerEditor.Instance.graphAsset.main is not BehaviorTree tree)
            {
                return;
            }
            
            if (_control.TryConnectNodesByEdge(_graphView, sourceNodeView, targetNodeView))
            {
                BehaviorTree.ConnectNodes((BehaviorNodeBase)sourceNodeView.targetNode, (BehaviorNodeBase)targetNodeView.targetNode);
            }
        }
    }
}