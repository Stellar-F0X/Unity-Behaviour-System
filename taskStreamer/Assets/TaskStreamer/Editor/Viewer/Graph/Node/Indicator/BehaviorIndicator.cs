using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.Utility;

namespace TaskStreamer.Tool
{
    internal class BehaviorIndicator : NodeIndicatorBase
    {
        internal BehaviorIndicator(NodeViewBase nodeView, TSEditorSettings settings) : base(nodeView, settings)
        {
            this.ApplyBorderColorByState();
        }


        protected override void OnHighlightStart()
        {
            if (((BehaviorNodeBase)_nodeView.targetNode).nodeType != BehaviorNodeType.Root)
            {
                _nodeView.connectionEdges[UGUID.Empty].BringToFront();
            }
        }


        protected override void OnHighlightUpdate(float progress)
        {
            _nodeView.nodeBorder.style.SetBorderColor(_settings.nodeStatusGradient.Evaluate(progress));

            if (((BehaviorNodeBase)_nodeView.targetNode).nodeType != BehaviorNodeType.Root)
            {
                _nodeView.connectionEdges[UGUID.Empty].SetEdgeColor(_settings.edgeStatusGradient.Evaluate(progress));
            }
        }


        protected override void OnHighlightEnd()
        {
            this.ApplyBorderColorByState();
        }


        public override sealed void ApplyBorderColorByState()
        {
            base.ApplyBorderColorByState();
            
            switch (((BehaviorNodeBase)_nodeView.targetNode).status)    
            {
                case Status.Failure: _nodeView.nodeBorder.style.SetBorderColor(TSEditor.settings.failureNodeColor); break;

                case Status.Success: _nodeView.nodeBorder.style.SetBorderColor(TSEditor.settings.successNodeColor); break;
            }
        }
    }
}