using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> 선택된 그래프 요소의 세부 정보를 표시하고 관리하는 플로팅 인스펙터 뷰 클래스 </summary>
    [UxmlElement]
    internal partial class FloatingInspectorView : VisualElement
    {
        public FloatingInspectorView()
        {
            schedule.Execute(_ => this.DelayedInitialize()).StartingIn(0);
        }


        /// <summary>
        /// 마우스 왼쪽 버튼을 나타내는 상수. 클릭 이벤트 식별에 사용됩니다.
        /// </summary>
        private const int _LEFT_MOUSE_BUTTON = 0;

        // 리사이저 관련 변수들
        /// <summary>
        /// 리사이징 상태를 나타내는 변수로, 사용자가 리사이저를 드래그하여 크기 조정 중인지 여부를 나타냅니다.
        /// </summary>
        private bool _isResizing;

        /// <summary>
        /// 사용자가 타이틀 바를 드래그하고 있는지 여부를 나타내는 플래그 변수입니다.
        /// </summary>
        private bool _isDragging;

        /// <summary>
        /// 마우스 클릭 위치를 기준으로 리사이즈 시작 지점을 저장하는 변수입니다.
        /// </summary>
        private Vector2 _resizeStartPosition;

        /// <summary>
        /// 크기 조정 시작 시점의 크기를 저장하는 변수입니다.
        /// 초기 크기를 기준으로 UI 요소의 크기 변화를 계산하는 데 사용됩니다.
        /// </summary>
        private Vector2 _resizeStartSize;

        /// <summary>
        /// 드래그 시작 위치를 저장하는 데 사용되는 변수입니다.
        /// 타이틀바 드래그 동작의 계산에 사용됩니다.
        /// </summary>
        private Vector2 _dragOffset;

        /// <summary>
        /// 사용자 인터페이스 크기 조정을 위한 VisualElement를 나타냅니다.
        /// </summary>
        private VisualElement _resizer;

        /// <summary>
        /// 드래그와 위치 조정을 위해 사용되는 타이틀 바 UI 요소를 나타냅니다.
        /// </summary>
        private VisualElement _titleBar;

        /// <summary>
        /// 인스펙터 뷰의 스크롤 가능한 콘텐츠 영역을 나타내는 변수.
        /// </summary>
        private ScrollView _contentContainer;



        /// <summary> UI 구성 요소 초기화 및 이벤트 리스너 연결을 처리 </summary>
        private void DelayedInitialize()
        {
            _contentContainer = this.Q<ScrollView>("content-container");
            _titleBar = this.Q<VisualElement>("title-bar");
            _resizer = this.Q<VisualElement>("resizer");

            this.style.left = parent.contentRect.width - this.contentRect.width - 15;
            this.style.top = 15;

            _titleBar.RegisterCallback<MouseDownEvent>(this.OnTitleBarMouseDown);
            _titleBar.RegisterCallback<MouseMoveEvent>(this.OnTitleBarMouseMove);
            _titleBar.RegisterCallback<MouseUpEvent>(this.OnTitleBarMouseUp);

            _resizer.RegisterCallback<MouseDownEvent>(this.OnResizerMouseDown);
            _resizer.RegisterCallback<MouseMoveEvent>(this.OnResizerMouseMove);
            _resizer.RegisterCallback<MouseUpEvent>(this.OnResizerMouseUp);
            
            // 휠 스크롤 이벤트 등록
            this.RegisterCallback<WheelEvent>(this.OnWheelEvent);
        }


#region Inspector Logic

        /// <summary> 선택된 그래프 요소의 데이터를 기반으로 인스펙터 뷰를 갱신합니다. </summary>
        /// <param name="selectedElement"> 선택된 그래프 요소 </param>
        public void UpdateSelection(GraphElement selectedElement)
        {
            this.ClearInspector();

            if (selectedElement is null)
            {
                Debug.LogError("selectedElement is null");
            }
            else
            {
                this.CreateInspectorContent(selectedElement);
            }
        }


        /// <summary> 인스펙터 뷰 내용을 초기화합니다. (Obsolete: ClearInspector를 대신 사용하세요.) </summary>
        [Obsolete("Please use ClearInspector method")]
        public new void Clear()
        {
            this.ClearInspector();
        }


        /// <summary> 인스펙터 뷰의 내용을 초기화 및 모든 기존 데이터를 정리 </summary>
        public void ClearInspector()
        {
            _contentContainer?.Clear();
        }


        /// <summary> 인스펙터 뷰의 내용을 새로고침하고 필요한 경우 패널을 갱신 </summary>
        public void RefreshInspector()
        {
            if (_contentContainer is null || _contentContainer.enabledSelf == false)
            {
                Debug.LogWarning("Failed to refresh inspector: Content container is disabled");
                return;
            }

            foreach (VisualElement child in _contentContainer.Children())
            {
                if (child is IRefreshablePanel refreshablePanel)
                {
                    refreshablePanel.RefreshPanel();
                }
            }
        }


        /// <summary> 그래프 요소에 따라 적절한 인스펙터 콘텐츠를 생성 및 추가 </summary>
        /// <param name="graphElement"> 인스펙터 콘텐츠를 생성할 대상 그래프 요소 </param>
        private void CreateInspectorContent(GraphElement graphElement)
        {
            switch (graphElement)
            {
                case BehaviorNodeView bNodeView:
                {
                    _contentContainer.Add(new BasicSectionPanel(bNodeView.targetNode, bNodeView.onRenamingNode));
                    _contentContainer.Add(new FieldSectionPanel(bNodeView.variableHandles));
                    _contentContainer.Add(new ServiceContainerPanel(bNodeView.serviceList, bNodeView.variableHandlesDic));
                    break;
                }

                case StateNodeView sNodeView:
                {
                    _contentContainer.Add(new BasicSectionPanel(sNodeView.targetNode, sNodeView.onRenamingNode));
                    _contentContainer.Add(new FieldSectionPanel(sNodeView.variableHandles));
                    break;
                }

                case ArrowEdge edgeView:
                {
                    _contentContainer.Add(new BasicSectionPanel(edgeView.targetTransition, null));
                    _contentContainer.Add(new FieldSectionPanel(edgeView.variableHandles));
                    break;
                }
            }
        }

#endregion



#region Mouse Event And Calculate Position Logic

        /// <summary> 휠 스크롤 이벤트를 처리하여 이 VisualElement 내에서만 스크롤이 동작하도록 함 </summary>
        /// <param name="evt">휠 이벤트 데이터</param>
        private void OnWheelEvent(WheelEvent evt)
        {
            // 마우스가 이 VisualElement 영역 내에 있는지 확인
            if (this.worldBound.Contains(evt.mousePosition) == false)
            {
                return;
            }
            
            if (this._contentContainer != null)
            {
                ScrollView scrollView = this._contentContainer;
                Vector2 scrollOffset = scrollView.scrollOffset;
                
                float maxHeight = scrollView.verticalScroller.highValue;
                float moveHeight = scrollOffset.y + evt.delta.y;
                
                scrollOffset.y = Mathf.Clamp(moveHeight, 0, maxHeight);
                scrollView.scrollOffset = scrollOffset;
            }

            // 이벤트 전파를 중단하여 다른 요소에서 스크롤되지 않도록 함
            evt.StopPropagation();
        }



        /// <summary> 타이틀바 마우스 다운 이벤트를 처리하여 드래그 상태를 활성화 </summary>
        /// <param name="evt">마우스 다운 이벤트 데이터</param>
        private void OnTitleBarMouseDown(MouseDownEvent evt)
        {
            if (evt.button != _LEFT_MOUSE_BUTTON)
            {
                return;
            }

            this._dragOffset = evt.localMousePosition;
            this._isDragging = true;
            this._titleBar.CaptureMouse();

            evt.StopPropagation();
        }


        /// <summary> 타이틀바에서 마우스를 이동할 때 인스펙터 뷰 위치를 업데이트 </summary>
        /// <param name="evt">마우스 이동 이벤트 데이터</param>
        private void OnTitleBarMouseMove(MouseMoveEvent evt)
        {
            if (_isDragging == false || _titleBar.HasMouseCapture() == false)
            {
                return;
            }

            Vector2 parentMousePos = this.parent.WorldToLocal(evt.mousePosition);
            Vector2 newPosition = this.ConstrainPositionToParent(parentMousePos - _dragOffset);

            this.style.left = newPosition.x;
            this.style.top = newPosition.y;

            evt.StopPropagation();
        }


        /// <summary> 부모 영역 안으로 자식 요소의 위치를 제한 </summary>
        /// <param name="position">자식 요소의 현재 위치</param>
        /// <return>제한된 위치 값</return>
        private Vector2 ConstrainPositionToParent(Vector2 position)
        {
            if (this.parent != null)
            {
                Rect parentRect = this.parent.contentRect;
                Rect elementRect = this.contentRect;

                position.x = Mathf.Clamp(position.x, 0, Mathf.Max(0, parentRect.width - elementRect.width));
                position.y = Mathf.Clamp(position.y, 0, Mathf.Max(0, parentRect.height - elementRect.height));
            }

            return position;
        }


        /// <summary> 타이틀바 마우스 업 이벤트를 처리하여 드래그 상태를 종료 </summary>
        /// <param name="evt">마우스 업 이벤트 데이터</param>
        private void OnTitleBarMouseUp(MouseUpEvent evt)
        {
            if (_isDragging == false)
            {
                return;
            }

            this._isDragging = false;
            this._titleBar.ReleaseMouse();

            evt.StopPropagation();
        }


        /// <summary> 리사이저 마우스 다운 이벤트를 처리하여 드래그 시작 상태를 설정 </summary>
        /// <param name="evt">마우스 다운 이벤트 데이터</param>
        private void OnResizerMouseDown(MouseDownEvent evt)
        {
            if (evt.button != _LEFT_MOUSE_BUTTON)
            {
                return;
            }

            this._resizeStartPosition = evt.mousePosition;
            this._resizeStartSize = new Vector2(this.resolvedStyle.width, this.resolvedStyle.height);
            this._isResizing = true;
            this._resizer.CaptureMouse();

            evt.StopPropagation();
        }


        /// <summary> 리사이저 마우스 이동 이벤트 처리 </summary>
        /// <param name="evt">마우스 이동 이벤트 데이터</param>
        private void OnResizerMouseMove(MouseMoveEvent evt)
        {
            if (_isResizing == false || _resizer.HasMouseCapture() == false)
            {
                return;
            }

            Vector2 mouseDelta = evt.mousePosition - _resizeStartPosition;

            // 현재 크기에서 마우스 이동량만큼 변경
            float newWidth = _resizeStartSize.x + mouseDelta.x;
            float newHeight = _resizeStartSize.y + mouseDelta.y;

            // 최소 크기 제한
            newWidth = Mathf.Max(newWidth, this.style.minWidth.value.value);
            newHeight = Mathf.Max(newHeight, this.style.minHeight.value.value);

            // 부모 영역 내 최대 크기 제한
            if (this.parent != null)
            {
                Rect parentRect = this.parent.contentRect;
                Vector2 currentPosition = new Vector2(this.resolvedStyle.left, this.resolvedStyle.top);

                newWidth = Mathf.Min(newWidth, parentRect.width - currentPosition.x);
                newHeight = Mathf.Min(newHeight, parentRect.height - currentPosition.y);
            }

            this.style.width = newWidth;
            this.style.height = newHeight;

            evt.StopPropagation();
        }


        /// <summary> 리사이저 마우스 업 이벤트 처리 및 리사이징 동작 종료 </summary>
        /// <param name="evt">마우스 업 이벤트 데이터</param>
        private void OnResizerMouseUp(MouseUpEvent evt)
        {
            if (_isResizing == false)
            {
                return;
            }

            this._isResizing = false;
            this._resizer.ReleaseMouse();

            evt.StopPropagation();
        }

#endregion
    }
}