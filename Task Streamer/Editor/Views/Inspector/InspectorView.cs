using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> 인스펙터 요소를 확장하여 사용자 정의 인스펙터 인터페이스를 제공하는 클래스 </summary>
    [UxmlElement]
    public partial class InspectorView : InspectorElement
    {
        /// <summary> 인스펙터 뷰의 모든 내용을 제거 </summary>
        public void ClearInspector()
        {
            this.Clear();
        }


        /// <summary> 인스펙터 뷰의 내용을 새로고침 </summary>
        public void RefreshInspector()
        {
            if (this.childCount == 0 || this[0] is not TaskInspectorView taskInspectorView)
            {
                return;
            }
            
            taskInspectorView.RefreshAllFields();
        }


        /// <summary> 선택된 그래프 요소의 정보로 인스펙터 뷰를 업데이트 </summary>
        /// <param name="selectedElement">선택된 그래프 요소</param>
        public void UpdateSelection(GraphElement selectedElement)
        {
            this.ClearInspector();

            if (selectedElement is null)
            {
                Debug.LogError("selectedElement is null");
                return;
            }

            VisualElement inspectorContent = this.CreateInspectorContent(selectedElement);
            Debug.Assert(inspectorContent != null, "Created content is null");
            this.Add(inspectorContent);
        }


        /// <summary> 그래프 요소에 따라 적절한 인스펙터 콘텐츠 생성  </summary>
        /// <param name="graphElement">인스펙터 콘텐츠를 생성할 그래프 요소</param>
        /// <returns>생성된 인스펙터 콘텐츠를 반환하는 델리게이트</returns>
        private VisualElement CreateInspectorContent(GraphElement graphElement)
        {
            switch (graphElement)
            {
                case NodeViewBase nodeView: return new TaskInspectorView(nodeView.targetNode, nodeView.onRenamingNode, nodeView.fieldProperties);

                case ArrowEdge edgeView: return new TaskInspectorView(edgeView.targetTransition, null, edgeView.fieldProperties);

                default: throw new ArgumentException($"Unsupported graph element type: {graphElement?.GetType().FullName}");
            }
        }
    }
}