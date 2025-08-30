namespace TaskStreamer.Tool
{
    /// <summary>
    /// NodeIndicatorBase 클래스는 NodeView에서 하이라이트 효과 및 상태에 기반한 테두리 색상을 관리하기 위한 추상 클래스입니다.
    /// </summary>
    public abstract class NodeIndicatorBase
    {
        public NodeIndicatorBase(NodeViewBase nodeView, EditorSettings settings)
        {
            this._settings = settings;
            this._nodeView = nodeView;
            this._isHighlighting = false;
            this._lastCallCount = nodeView.targetNode.callCount;
        }

        /// 하이라이트 지속 시간이 _END_THRESHOLD 값(0.01f)보다 작아지면 하이라이트 상태를 종료시키는 데 사용됩니다.
        private const float _END_THRESHOLD = 0.01f;

        /// 노드가 강조(Highlight) 중인지 여부를 나타내는 플래그 변수입니다.
        /// 강조 상태 전환 및 업데이트 로직에서 사용됩니다.
        private bool _isHighlighting;

        /// 현재 하이라이트 상태에서 남은 시간을 나타내는 변수로, float 형식이며 단위는 초(Seconds)입니다.
        /// Highlight 메서드 호출 시 업데이트되며, 하이라이트 종료를 결정하는 데 사용됩니다.
        private float _remainingTime;

        /// 노드의 마지막 호출 횟수를 저장하는 변수입니다. 현재 노드가 갱신되었는지 확인하기 위해 사용됩니다.
        private ulong _lastCallCount;

        /// NodeIndicatorBase 클래스의 파생 클래스에서 사용되는 protected 변수로, NodeViewBase 객체를 참조하여 노드의 시각적 및 상태적 처리를 지원합니다.
        protected NodeViewBase _nodeView;

        /// NodeIndicatorBase에서 사용되는 설정 정보로, 에디터 내 노드 강조 효과의 동작 방식을 정의합니다.
        protected EditorSettings _settings;




        /// 노드가 하이라이트될 수 있는 상태인지 여부를 반환합니다.
        /// <return> 현재 노드가 하이라이트 될 수 있는 경우 true, 그렇지 않으면 false.</return>
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


        /// <summary>
        /// 노드의 하이라이트 상태를 업데이트합니다.
        /// 노드가 호출되었을 경우 하이라이트를 시작하고, 진행률에 따라 상태를 업데이트한 후 종료합니다.
        /// </summary>
        /// <param name="deltaTime">하이라이트 상태를 업데이트하는 데 사용할 시간 간격입니다.</param>
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

            this.OnHighlightUpdate(_remainingTime * _settings.durationReciprocal);

            _remainingTime -= deltaTime;

            if (_remainingTime < _END_THRESHOLD)
            {
                _isHighlighting = false;
                this.OnHighlightEnd();
            }
        }


        /// 노드 상태에 따라 테두리 색상을 설정하는 메서드입니다.
        /// 기본 또는 확장된 클래스에서 노드 상태를 확인하고 적합한 색상으로 변경합니다.
        public virtual void ApplyBorderColorByState()
        {
            if (_nodeView.targetNode is null || _nodeView.targetNode.callCount == 0)
            {
                return;
            }
            
            if (_nodeView.targetNode.callState != NodeCallState.BeforeEnter)
            {
                return;
            }

            _nodeView.nodeBorder.style.SetBorderColor(_settings.nodeStatusGradient.Evaluate(0));
        }


        /// <summary>
        /// 하이라이트가 시작될 때 호출되는 메서드로, 상태별로 오버라이드하여 특수 동작을 구현합니다.
        /// </summary>
        protected abstract void OnHighlightStart();


        /// <summary>
        /// 하이라이트 진행 상황을 업데이트하는 메서드.
        /// </summary>
        /// <param name="progress">하이라이트 진행률 (0 ~ 1 범위의 값).</param>
        protected abstract void OnHighlightUpdate(float progress);


        /// 하이라이트가 종료될 때 호출되는 메서드로, 하위 클래스에서 구체적인 동작을 정의합니다.
        /// 재정의를 통해 노드 상태를 갱신하거나 후처리를 수행할 수 있습니다.
        protected abstract void OnHighlightEnd();
    }
}