namespace TaskStreamer.Tool
{
    public class StateNodeIndicator : NodeIndicatorBase
    {
        public StateNodeIndicator(NodeViewBase nodeView, EditorSettings settings) : base(nodeView, settings) { }


        protected override void OnHighlightStart() { }


        protected override void OnHighlightUpdate(float progress)
        {
            _nodeView.nodeBorder.style.SetBorderColor(_settings.nodeStatusGradient.Evaluate(progress));
        }


        protected override void OnHighlightEnd() { }
    }
}