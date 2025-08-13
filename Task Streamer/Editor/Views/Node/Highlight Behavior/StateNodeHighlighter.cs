namespace TaskStreamer.Tool
{
    public class StateNodeHighlighter : NodeHighlighterBase
    {
        public StateNodeHighlighter(NodeViewBase nodeView, EditorSettings settings) : base(nodeView, settings) { }


        protected override void OnHighlightStart() { }


        protected override void OnHighlightUpdate(float remainingHighlightTime)
        {
            float progress = remainingHighlightTime / _settings.highlightDuration;

            _nodeView.nodeBorder.style.SetBorderColor(_settings.nodeStatusLinearColor.Evaluate(progress));
        }


        protected override void OnHighlightEnd() { }
    }
}