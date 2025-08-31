using System.Collections.Generic;
using System.Linq;
using TaskStreamer.BT;
using TaskStreamer.FSM;
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


        /// <summary> 인스펙터 뷰의 내용을 초기화 및 정리 </summary>
        public void ClearInspector()
        {
            this.Clear();
        }


        /// <summary> 인스펙터 뷰의 내용을 새로고침 </summary>
        public void RefreshInspector()
        {
            if (this.childCount == 0)
            {
                return;
            }

            foreach (VisualElement child in this.Children())
            {
                if (child is IRefreshablePanel refreshablePanel)
                {
                    refreshablePanel.RefreshPanel();
                }
            }
        }


        /// <summary> 그래프 요소에 적합한 인스펙터 콘텐츠를 생성 </summary>
        /// <param name="graphElement"> 인스펙터 콘텐츠를 생성할 대상 그래프 요소 </param>
        private void CreateInspectorContent(GraphElement graphElement)
        {
            switch (graphElement)
            {
                case BehaviorNodeView bNodeView: //behavior node view
                {
                    this.Add(new BasicSectionPanel(bNodeView.targetNode, bNodeView.onRenamingNode));
                    this.Add(new FieldSectionPanel(bNodeView.variableHandles.Where(f => f.initialValue is not List<ServiceBase>).ToList()));
                    this.Add(new ServiceContainerPanel(bNodeView.variableHandles.FirstOrDefault(f => f.initialValue is List<ServiceBase>)));
                    break;
                }

                case StateNodeView sNodeView: //state node view
                {
                    this.Add(new BasicSectionPanel(sNodeView.targetNode, sNodeView.onRenamingNode));
                    this.Add(new FieldSectionPanel(sNodeView.variableHandles.Where(f => f.initialValue is not List<Transition>).ToList()));
                    break;
                }

                case ArrowEdge edgeView:
                {
                    this.Add(new BasicSectionPanel(edgeView.targetTransition, null));
                    this.Add(new FieldSectionPanel(edgeView.variableHandles));
                    return;
                }
            }
        }
    }
}