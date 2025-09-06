using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> 선택된 그래프 요소의 데이터를 기반으로 플로팅 인스펙터를 갱신 및 관리하는 클래스 </summary>
    internal class FloatingInspector : FloatingPanel
    {
        public FloatingInspector()
        {
            TaskStreamerResourceLoader.FloatingInspector.CloneTree(this);
            
            this.AddToClassList("inspector-window");
        }
        
        
        public BasicSection basicPanel
        {
            get { return container[0] as BasicSection; }
        }

        
        public FieldSection fieldContainerPanel
        {
            get { return container[1] as FieldSection; }
        }

        
        public ServiceContainer serviceContainerPanel
        {
            get { return container[2] as ServiceContainer; }
        }




        /// <summary> 선택된 그래프 요소의 데이터를 기반으로 플로팅 인스펙터 뷰를 갱신합니다. </summary>
        /// <param name="selectedElement"> 선택된 그래프 요소 </param>
        public void UpdateSelection(GraphElement selectedElement)
        {
            if (selectedElement is null)
            {
                Debug.LogError("selectedElement is null");
                return;
            }
            
            if (container is null)
            {
                Debug.LogError("Failed to refresh inspector: Content container is disabled");
                return;
            }

            if (container.enabledSelf == false)
            {
                return;
            }

            if (container.childCount == 0)
            {
                this.CreateInspectorContent(selectedElement);
            }
            else
            {
                this.RefreshInspectorWithNewValue(selectedElement);
            }
        }



        /// <summary> 인스펙터 내용을 초기화합니다. ClearInspector 메서드 대신 사용을 권장하지 않습니다. </summary>
        [Obsolete("Please use ClearInspector method")]
        public new void Clear()
        {
            this.ClearInspector();
        }



        /// <summary> 인스펙터 뷰의 데이터를 초기화하고 필요시 모든 요소를 강제 제거합니다. </summary>
        /// <param name="force"> 모든 데이터를 강제로 제거할지 여부 </param>
        public void ClearInspector(bool force = false)
        {
            if (container is null || container.childCount == 0)
            {
                return;
            }

            //데이터가 이미 지워진 경우, 강제로 지우지 않으면 오류가 발생할 수도 있다. 
            if (force)
            {
                container.Clear();
            }
            else
            {
                container[0].style.display = DisplayStyle.None;
                container[1].style.display = DisplayStyle.None;
                container[2].style.display = DisplayStyle.None;
            }
        }



        /// <summary> 그래프 변경 사항에 따라 인스펙터 뷰를 새로고침합니다. </summary>
        public void RefreshInspector()
        {
            if (container is null || container.enabledSelf == false)
            {
                Debug.LogError("Failed to refresh inspector: Content container is disabled");
                return;
            }

            foreach (VisualElement child in container.Children())
            {
                if (child is IRefreshablePanel refreshablePanel)
                {
                    refreshablePanel.RefreshPanel();
                }
            }
        }



        /// <summary> 주어진 그래프 요소의 데이터를 기반으로 플로팅 인스펙터 뷰의 패널을 갱신합니다. </summary>
        /// <param name="graphElement"> 갱신에 사용될 그래프 요소 </param>
        private void RefreshInspectorWithNewValue(GraphElement graphElement)
        {
            basicPanel.style.display = DisplayStyle.Flex;
            fieldContainerPanel.style.display = DisplayStyle.Flex;
            
            switch (graphElement)
            {
                case BehaviorNodeView bNodeView:
                {
                    basicPanel.RefreshPanelWithNewValue(bNodeView);
                    fieldContainerPanel.RefreshPanelWithNewValue(bNodeView.variableHandles);
                    serviceContainerPanel.RefreshPanelWithNewValue((bNodeView.serviceList, bNodeView.variableHandlesDic));
                    serviceContainerPanel.style.display = DisplayStyle.Flex;
                    break;
                }

                case StateNodeView sNodeView:
                {
                    basicPanel.RefreshPanelWithNewValue(sNodeView);
                    fieldContainerPanel.RefreshPanelWithNewValue(sNodeView.variableHandles);
                    serviceContainerPanel.style.display = DisplayStyle.None;
                    break;
                }

                case ArrowEdge edgeView:
                {
                    basicPanel.RefreshPanelWithNewValue(edgeView);
                    fieldContainerPanel.RefreshPanelWithNewValue(edgeView.variableHandles);
                    serviceContainerPanel.style.display = DisplayStyle.None;
                    break;
                }
            }
        }



        /// <summary> 그래프 요소 타입에 적합한 인스펙터 콘텐츠를 생성 및 추가합니다. </summary>
        /// <param name="graphElement"> 콘텐츠를 생성할 대상 그래프 요소 </param>
        private void CreateInspectorContent(GraphElement graphElement)
        {
            switch (graphElement)
            {
                case BehaviorNodeView bNodeView:
                {
                    container.Add(new BasicSection(bNodeView.targetNode, bNodeView.onRenamingNode));
                    container.Add(new FieldSection(bNodeView.variableHandles));
                    container.Add(new ServiceContainer(bNodeView.serviceList, bNodeView.variableHandlesDic));
                    break;
                }

                case StateNodeView sNodeView:
                {
                    container.Add(new BasicSection(sNodeView.targetNode, sNodeView.onRenamingNode));
                    container.Add(new FieldSection(sNodeView.variableHandles));
                    container.Add(new ServiceContainer() { style = { display = DisplayStyle.None } });
                    break;
                }

                case ArrowEdge edgeView:
                {
                    container.Add(new BasicSection(edgeView.targetTransition, null));
                    container.Add(new FieldSection(edgeView.variableHandles));
                    container.Add(new ServiceContainer() { style = { display = DisplayStyle.None } });
                    break;
                }
            }
        }
    }
}