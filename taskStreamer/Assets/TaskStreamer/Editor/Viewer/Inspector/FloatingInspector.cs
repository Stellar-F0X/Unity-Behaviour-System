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
			TSEditor.floatingInspector.CloneTree(this);
			this.AddToClassList("inspector-window");
		}

		private GraphElement _focusedElement;


		public GraphElement focusedElement
		{
			get { return _focusedElement; }
		}

		private BasicPropertiesSection basicPropertiesPanel
		{
			get { return container[0] as BasicPropertiesSection; }
		}

		private FieldPropertiesSection fieldPropertiesPanel
		{
			get { return container[1] as FieldPropertiesSection; }
		}

		private ServiceContainer serviceContainerPanel
		{
			get { return container[2] as ServiceContainer; }
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

			container.Children()
			         .OfType<IRefreshablePanel>()
			         .ForEach(child => child.RefreshPanel());
		}



		/// <summary> 주어진 그래프 요소의 데이터를 기반으로 플로팅 인스펙터 뷰의 패널을 갱신합니다. </summary>
		/// <param name="graphElement"> 갱신에 사용될 그래프 요소 </param>
		private void RefreshInspectorWithNewValue(GraphElement graphElement)
		{
			switch (graphElement)
			{
				case BehaviorNodeView bNodeView:
				{
					basicPropertiesPanel.RefreshPanelWithNewValue(bNodeView);
					fieldPropertiesPanel.RefreshPanelWithNewValue(bNodeView.targetNode.variableHandles);
					serviceContainerPanel.RefreshPanelWithNewValue(bNodeView.observableServiceList);
					serviceContainerPanel.style.display = DisplayStyle.Flex;
					break;
				}

				case StateNodeView sNodeView:
				{
					serviceContainerPanel.style.display = DisplayStyle.None;
					basicPropertiesPanel.RefreshPanelWithNewValue(sNodeView);
					fieldPropertiesPanel.RefreshPanelWithNewValue(sNodeView.targetNode.variableHandles);
					break;
				}

				case FSMEdge edgeView:
				{
					serviceContainerPanel.style.display = DisplayStyle.None;
					basicPropertiesPanel.RefreshPanelWithNewValue(edgeView);
					fieldPropertiesPanel.RefreshPanelWithNewValue(edgeView.targetTransition.variableHandles);
					break;
				}
			}

			basicPropertiesPanel.style.display = DisplayStyle.Flex;
			fieldPropertiesPanel.style.display = DisplayStyle.Flex;
		}



		/// <summary> 그래프 요소 타입에 적합한 인스펙터 콘텐츠를 생성 및 추가합니다. </summary>
		/// <param name="graphElement"> 콘텐츠를 생성할 대상 그래프 요소 </param>
		private void CreateInspectorContent(GraphElement graphElement)
		{
			switch (graphElement)
			{
				case BehaviorNodeView bNodeView:
				{
					container.Add(new BasicPropertiesSection(bNodeView.targetNode, bNodeView.onRenamingNode));
					container.Add(new FieldPropertiesSection(bNodeView.targetNode.variableHandles));
					container.Add(new ServiceContainer(bNodeView.observableServiceList));
					break;
				}

				case StateNodeView sNodeView:
				{
					container.Add(new BasicPropertiesSection(sNodeView.targetNode, sNodeView.onRenamingNode));
					container.Add(new FieldPropertiesSection(sNodeView.targetNode.variableHandles));
					container.Add(new ServiceContainer() { style = { display = DisplayStyle.None } });
					break;
				}

				case FSMEdge edgeView:
				{
					container.Add(new BasicPropertiesSection(edgeView.targetTransition, null));
					container.Add(new FieldPropertiesSection(edgeView.targetTransition.variableHandles));
					container.Add(new ServiceContainer() { style = { display = DisplayStyle.None } });
					break;
				}
			}
		}
	}
}