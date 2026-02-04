using System;
using System.Linq;
using TaskStreamer.Runtime.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
	/// <summary> 선택된 그래프 요소의 데이터를 기반으로 플로팅 인스펙터를 갱신 및 관리하는 클래스 </summary>
	internal class FloatingInspector : FloatingPanel
	{
		public FloatingInspector()
		{
			TSUIElementSettings.instance.Inspector.CloneTree(this);
			this.AddToClassList("inspector-window");
		}

		private GraphElement _focusedElement;


		public GraphElement focusedElement
		{
			get { return _focusedElement; }
		}

		private TaskHeaderSection taskHeaderSection
		{
			get { return container[0] as TaskHeaderSection; }
		}

		private BBVariableFieldsPanel bbVariableFieldsPanel
		{
			get { return container[1] as BBVariableFieldsPanel; }
		}

		private ServiceSectionsPanel serviceSectionsPanel
		{
			get { return container[2] as ServiceSectionsPanel; }
		}




		/// <summary> 선택된 그래프 요소의 데이터를 기반으로 플로팅 인스펙터 뷰를 갱신합니다. </summary>
		/// <param name="selectedElement"> 선택된 그래프 요소 </param>
		public void UpdateSelection(GraphElement selectedElement)
		{
			Assert.IsNotNull(selectedElement, "selectedElement is null");
			Assert.IsNotNull(container, "Failed to refresh inspector: Content container is disabled");

			if (container.enabledSelf == false)
			{
				return;
			}

			_focusedElement = selectedElement;

			if (container.childCount == 0)
			{
				this.CreateInspector(selectedElement);
			}
			else
			{
				this.UpdateInspector(selectedElement);
			}
		}
		
		
		
		/// <summary> 주어진 그래프 요소의 데이터를 기반으로 플로팅 인스펙터 뷰의 패널을 갱신합니다. </summary>
		/// <param name="graphElement"> 갱신에 사용될 그래프 요소 </param>
		private void UpdateInspector(GraphElement graphElement)
		{
			switch (graphElement)
			{
				case BehaviorNodeView bNodeView:
				{
					taskHeaderSection.RefreshPanelWithNewValue(bNodeView);
					bbVariableFieldsPanel.RefreshPanelWithNewValue(bNodeView.targetNode.variableHandles);
					serviceSectionsPanel.RefreshPanelWithNewValue(bNodeView.observableServiceList);
					serviceSectionsPanel.style.display = DisplayStyle.Flex;
					break;
				}

				case StateNodeView sNodeView:
				{
					serviceSectionsPanel.style.display = DisplayStyle.None;
					taskHeaderSection.RefreshPanelWithNewValue(sNodeView);
					bbVariableFieldsPanel.RefreshPanelWithNewValue(sNodeView.targetNode.variableHandles);
					break;
				}

				case FSMEdge edgeView:
				{
					serviceSectionsPanel.style.display = DisplayStyle.None;
					taskHeaderSection.RefreshPanelWithNewValue(edgeView);
					bbVariableFieldsPanel.RefreshPanelWithNewValue(edgeView.targetTransition.variableHandles);
					break;
				}
			}

			taskHeaderSection.style.display = DisplayStyle.Flex;
			bbVariableFieldsPanel.style.display = DisplayStyle.Flex;
		}



		/// <summary> 그래프 요소 타입에 적합한 인스펙터 콘텐츠를 생성 및 추가합니다. </summary>
		/// <param name="graphElement"> 콘텐츠를 생성할 대상 그래프 요소 </param>
		private void CreateInspector(GraphElement graphElement)
		{
			switch (graphElement)
			{
				case BehaviorNodeView bNodeView:
				{
					container.Add(new TaskHeaderSection(bNodeView.targetNode, bNodeView.onRenamingNode));
					container.Add(new BBVariableFieldsPanel(bNodeView.targetNode.variableHandles));
					container.Add(new ServiceSectionsPanel(bNodeView.observableServiceList));
					break;
				}

				case StateNodeView sNodeView:
				{
					container.Add(new TaskHeaderSection(sNodeView.targetNode, sNodeView.onRenamingNode));
					container.Add(new BBVariableFieldsPanel(sNodeView.targetNode.variableHandles));
					container.Add(new ServiceSectionsPanel() { style = { display = DisplayStyle.None } });
					break;
				}

				case FSMEdge edgeView:
				{
					container.Add(new TaskHeaderSection(edgeView.targetTransition, null));
					container.Add(new BBVariableFieldsPanel(edgeView.targetTransition.variableHandles));
					container.Add(new ServiceSectionsPanel() { style = { display = DisplayStyle.None } });
					break;
				}
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
			_focusedElement = null;

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
			Assert.IsNotNull(container, "Failed to refresh inspector: Content container is disabled");
			Assert.IsTrue(container.enabledSelf, "Cannot refresh inspector when container is disabled");

			foreach (IRefreshablePanel panel in container.Children())
			{
				panel?.RefreshPanel();
			}
		}
	}
}