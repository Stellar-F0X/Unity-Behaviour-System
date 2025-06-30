using TaskStreamer.BT;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class BTEdgeConnectorListener : GraphEdgeConnectorListener
    {
        public BTEdgeConnectorListener()
        {
            _processor = TaskStreamerEditor.Instance.view.graphViewProcessor;
            _graphView = TaskStreamerEditor.Instance.view;
        }
        
        private readonly GraphViewProcessor _processor;
        
        private readonly TaskGraphView _graphView;
        
        
        protected override void CreateAndLinkFromOriginalToNewNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position)
        {
            _processor.TryDisconnectParentToChild(sourceNodeView);

            if (TaskStreamerEditor.Instance.graphAsset.main is not BehaviorTree)
            {
                return;
            }

            if (_processor.TryConnectNodesByEdge(_graphView, sourceNodeView, targetNodeView, out _))
            {
                BehaviorTree.ConnectNodes((BehaviorNodeBase)sourceNodeView.targetNode, (BehaviorNodeBase)targetNodeView.targetNode);
            }
        }


        protected override void CreateAndLinkFromNewToOriginalNode(NodeView sourceNodeView, NodeView targetNodeView, Vector2 position)
        {
            _processor.TryDisconnectChildToParent(targetNodeView);

            if (TaskStreamerEditor.Instance.graphAsset.main is not BehaviorTree tree)
            {
                return;
            }
            
            if (_processor.TryConnectNodesByEdge(_graphView, sourceNodeView, targetNodeView, out _))
            {
                BehaviorTree.ConnectNodes((BehaviorNodeBase)sourceNodeView.targetNode, (BehaviorNodeBase)targetNodeView.targetNode);
            }
        }
    }
}