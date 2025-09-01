using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> 인스펙터 요소를 확장하여 사용자 정의 인스펙터 인터페이스를 제공하는 클래스 </summary>
    [UxmlElement]
    public partial class FloatingInspectorView : VisualElement
    {
        public FloatingInspectorView()
        {
            schedule.Execute(_ => DelayedInitialize()).StartingIn(0);
        }


        private const int _LEFT_MOUSE_BUTTON = 0;

        // 리사이저 관련 변수들
        private bool _isResizing;
        private bool _isDragging;
        
        private Vector2 _resizeStartPosition;
        private Vector2 _resizeStartSize;
        private Vector2 _dragOffset;
        
        private VisualElement _resizer;
        
        private VisualElement _titleBar;
        
        private ScrollView _contentContainer;



        private void DelayedInitialize()
        {
            _contentContainer = this.Q<ScrollView>("content-container");
            _titleBar = this.Q<VisualElement>("title-bar");
            _resizer = this.Q<VisualElement>("resizer");

            this.style.left = parent.contentRect.width - this.contentRect.width - 5;
            this.style.top = 5;

            _titleBar.RegisterCallback<MouseDownEvent>(this.OnTitleBarMouseDown);
            _titleBar.RegisterCallback<MouseMoveEvent>(this.OnTitleBarMouseMove);
            _titleBar.RegisterCallback<MouseUpEvent>(this.OnTitleBarMouseUp);

            _resizer.RegisterCallback<MouseDownEvent>(this.OnResizerMouseDown);
            _resizer.RegisterCallback<MouseMoveEvent>(this.OnResizerMouseMove);
            _resizer.RegisterCallback<MouseUpEvent>(this.OnResizerMouseUp);
        }


        /// <summary> 선택된 그래프 요소 정보를 기반으로 인스펙터 뷰를 업데이트 </summary>
        /// <param name="selectedElement">선택된 그래프 요소</param>
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


        [Obsolete("Please use ClearInspector method")]
        public new void Clear()
        {
            this.ClearInspector();
        }


        /// <summary> 인스펙터 뷰의 내용을 초기화 및 정리 </summary>
        public void ClearInspector()
        {
            _contentContainer?.Clear();
        }


        /// <summary> 인스펙터 뷰의 내용을 새로고침 </summary>
        public void RefreshInspector()
        {
            if (_contentContainer?.enabledSelf == false)
            {
                Debug.LogWarning("Failed to refresh inspector: Content container is disabled");
                return;
            }

            if (_contentContainer != null)
            {
                foreach (VisualElement child in _contentContainer.Children())
                {
                    if (child is IRefreshablePanel refreshablePanel)
                    {
                        refreshablePanel.RefreshPanel();
                    }
                }
            }
        }


        /// <summary> 타이틀바 마우스 다운 이벤트 처리 </summary>
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


        /// <summary> 타이틀바 마우스 이동 이벤트 처리 </summary>
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


        /// <summary> 위치를 부모 영역 내로 제한 </summary>
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


        /// <summary> 타이틀바 마우스 업 이벤트 처리 </summary>
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


        /// <summary> 리사이저 마우스 다운 이벤트 처리 </summary>
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


        /// <summary> 리사이저 마우스 업 이벤트 처리 </summary>
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


        /// <summary> 그래프 요소에 적합한 인스펙터 콘텐츠를 생성 </summary>
        /// <param name="graphElement"> 인스펙터 콘텐츠를 생성할 대상 그래프 요소 </param>
        private void CreateInspectorContent(GraphElement graphElement)
        {
            switch (graphElement)
            {
                case BehaviorNodeView bNodeView:
                {
                    _contentContainer.Add(new BasicSectionPanel(bNodeView.targetNode, bNodeView.onRenamingNode));
                    _contentContainer.Add(new FieldSectionPanel(bNodeView.variableHandles.GetRange(1, bNodeView.variableHandles.Count - 1)));
                    _contentContainer.Add(new ServiceContainerPanel(bNodeView.variableHandles[0]));
                    break;
                }

                case StateNodeView sNodeView:
                {
                    _contentContainer.Add(new BasicSectionPanel(sNodeView.targetNode, sNodeView.onRenamingNode));
                    _contentContainer.Add(new FieldSectionPanel(sNodeView.variableHandles.GetRange(1, sNodeView.variableHandles.Count - 1)));
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
    }
}