namespace TaskStreamer.Tool
{
    internal class StateNodeIndicator : NodeIndicatorBase
    {
        internal StateNodeIndicator(NodeViewBase nodeView, TSEditorSettings settings) : base(nodeView, settings) { }


        protected override void OnHighlightStart() { }


        protected override void OnHighlightUpdate(float progress)
        {
            _nodeView.nodeBorder.style.SetBorderColor(_settings.nodeStatusGradient.Evaluate(progress));
        }


        protected override void OnHighlightEnd() { }
    }
}