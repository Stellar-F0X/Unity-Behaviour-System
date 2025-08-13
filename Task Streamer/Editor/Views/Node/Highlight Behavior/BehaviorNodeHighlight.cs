using TaskStreamer.BT;
using TaskStreamer.Utility;

namespace TaskStreamer.Tool
{
    public class BehaviorNodeHighlight : NodeHighlighterBase
    {
        public BehaviorNodeHighlight(NodeViewBase nodeView, EditorSettings settings) : base(nodeView, settings) { }


        protected override void OnHighlightStart()
        {
            if (((BehaviorNodeBase)_nodeView.targetNode).nodeType != BehaviorNodeType.Root)
            {
                _nodeView.connectionEdge[UGUID.Empty].BringToFront();
            }
        }


        protected override void OnHighlightUpdate(float remainingHighlightTime)
        {
            float progress = remainingHighlightTime / _settings.highlightDuration;

            _nodeView.nodeBorder.style.SetBorderColor(_settings.nodeStatusLinearColor.Evaluate(progress));

            if (((BehaviorNodeBase)_nodeView.targetNode).nodeType != BehaviorNodeType.Root)
            {
                _nodeView.connectionEdge[UGUID.Empty].SetEdgeColor(_settings.edgeStatusLinearColor.Evaluate(progress));
            }
        }


        protected override void OnHighlightEnd()
        {
            this.ApplyBorderColorByState();
        }


        public override void ApplyBorderColorByState()
        {
            base.ApplyBorderColorByState();
            
            switch (((BehaviorNodeBase)_nodeView.targetNode).status)
            {
                case Status.Failure: _nodeView.nodeBorder.style.SetBorderColor(TaskStreamerEditor.settings.nodeFailureColor); break;

                case Status.Success: _nodeView.nodeBorder.style.SetBorderColor(TaskStreamerEditor.settings.nodeSuccessColor); break;
            }
        }
    }
}