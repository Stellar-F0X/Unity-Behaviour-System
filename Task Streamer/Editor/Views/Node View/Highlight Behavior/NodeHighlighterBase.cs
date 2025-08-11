namespace TaskStreamer.Tool
{
    public abstract class NodeHighlighterBase
    {
        public NodeHighlighterBase(NodeViewBase nodeView, EditorSettings settings)
        {
            this._settings = settings;
            this._nodeView = nodeView;
            this._isHighlighting = false;
            this._lastCallCount = nodeView.targetNode.callCount;
        }

        private const float _END_THRESHOLD = 0.02f;

        private bool _isHighlighting;
        private float _remainingTime;
        private ulong _lastCallCount;

        protected NodeViewBase _nodeView;
        protected EditorSettings _settings;


        public bool isHighlighting
        {
            get { return _isHighlighting; }
        }

        public float remainingTime
        {
            get { return _remainingTime; }
        }

        public ulong lastCallCount
        {
            get { return _lastCallCount; }
        }


        public virtual bool CanHighlight()
        {
            if (_nodeView is null || _nodeView.targetNode == null)
            {
                return false;
            }

            // 하이라이트되지 않은 상태임과 동시에 노드 호출 횟수가 변경되지 않았다면 업데이트가 불필요하다.
            if (_isHighlighting == false && _nodeView.targetNode.callCount == _lastCallCount)
            {
                return false;
            }

            return true;
        }


        public virtual void Highlight(float deltaTime)
        {
            if (_nodeView.targetNode.callCount > _lastCallCount)
            {
                if (_isHighlighting == false)
                {
                    _isHighlighting = true;
                    this.OnHighlightStart();
                }

                _remainingTime = _settings.highlightDuration; 
                _lastCallCount = _nodeView.targetNode.callCount; 
            }

            this.OnHighlightUpdate(_remainingTime);

            _remainingTime -= deltaTime;

            if (_remainingTime < _END_THRESHOLD)
            {
                _isHighlighting = false;
                this.OnHighlightEnd();
            }
        }


        protected abstract void OnHighlightStart();


        protected abstract void OnHighlightUpdate(float remainingHighlightTime);


        protected abstract void OnHighlightEnd();
    }
}