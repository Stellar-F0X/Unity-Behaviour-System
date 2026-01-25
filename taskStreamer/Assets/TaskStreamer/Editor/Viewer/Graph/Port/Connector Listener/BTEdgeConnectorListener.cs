using TaskStreamer.Runtime.BT;
using UnityEngine;

namespace TaskStreamer.Tool
{
    internal class BTEdgeConnectorListener : GraphEdgeConnectorListener
    {
        protected override void CreationAndLinkAToB(NodeViewBase sourceView, NodeViewBase newView, Vector2 position)
        {
            ((BTView)_taskView.graphView).TryDisconnectParentToChild(sourceView);

            if (_taskView.graphView.TryConnectNodesByEdge(_taskView, sourceView, newView))
            {
                BehaviorTree behaviorTree = TSEditor.Instance.currentGraph as BehaviorTree;
                Debug.Assert(behaviorTree is not null, "behaviorTree is null");
                behaviorTree.ConnectNodes((BehaviorNodeBase)sourceView.targetNode, (BehaviorNodeBase)newView.targetNode);
            }
        }

        
        protected override void CreationAndLinkBToA(NodeViewBase newView, NodeViewBase sourceView, Vector2 position)
        {
            ((BTView)_taskView.graphView).TryDisconnectChildToParent(sourceView);

            if (_taskView.graphView.TryConnectNodesByEdge(_taskView, newView, sourceView))
            {
                BehaviorTree behaviorTree = TSEditor.Instance.currentGraph as BehaviorTree;
                Debug.Assert(behaviorTree is not null, "behaviorTree is null");
                behaviorTree.ConnectNodes((BehaviorNodeBase)newView.targetNode, (BehaviorNodeBase)sourceView.targetNode);
            }
        }
    }
}