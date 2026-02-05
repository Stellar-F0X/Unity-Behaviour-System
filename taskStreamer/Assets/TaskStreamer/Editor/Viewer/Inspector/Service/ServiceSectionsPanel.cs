using System;
using System.Collections.Generic;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
	/// <summary>
	/// ServiceContainer 클래스는 VisualElement를 상속받아 서비스 리스트를 UI로 관리하며,
	/// IRefreshablePanel 인터페이스를 구현하여 동적 갱신 기능을 제공합니다.
	/// </summary>
	internal class ServiceSectionsPanel : VisualElement, IRefreshablePanel
	{
		public ServiceSectionsPanel()
		{
			TSUIElementSettings.instance.ServiceSectionsPanel.CloneTree(this);

			_serviceListView = this.Q<ListView>("service-list");
			_serviceListView.makeItem += this.MakeItem;
			_serviceListView.bindItem += this.BindServiceItem;

			Button addServiceButton = this.Q<Button>("add-button");
			addServiceButton.clickable.clickedWithEventInfo += this.OnAddServiceButtonClicked;
			addServiceButton.enabledSelf = TSEditor.canEditGraph;
		}


		public ServiceSectionsPanel(ObservableList<ServiceBase> serviceList) : this()
		{
			this.RefreshPanelWithNewValue(serviceList);
			this.RefreshPanel();
		}

		/// _serviceListView 변수는 서비스 목록을 표시하고 관리하기 위한 ListView UI 요소입니다.
		/// _serviceList 데이터를 바인딩하여 항목 표시 및 갱신 작업을 수행합니다.
		private readonly ListView _serviceListView;



		/// <summary> 서비스 목록과 UI 패널을 새로고침하여 현재 상태를 반영합니다. </summary>
		public void RefreshPanel()
		{
			if (TSEditor.Instance.currentGraph.graphType != GraphType.BT)
			{
				return;
			}

			Assert.IsNotNull(_serviceListView.itemsSource, $"{typeof(ServiceSectionsPanel)}'s itemsSource is null");

			if (_serviceListView.itemsSource.Count > 0)
			{
				_serviceListView.style.display = DisplayStyle.Flex;
				_serviceListView.RefreshItems();
			}
			else
			{
				_serviceListView.style.display = DisplayStyle.None;
			}
		}



		public void RefreshPanelWithNewValue(object newValue)
		{
			if (TSEditor.Instance.currentGraph.graphType != GraphType.BT)
			{
				return;
			}

			if (newValue is not ObservableList<ServiceBase> serviceList)
			{
				Debug.LogError("newValue is invalid");
				return;
			}

			_serviceListView.itemsSource = null;
			_serviceListView.Clear();

			if (serviceList.Count > 0)
			{
				_serviceListView.style.display = DisplayStyle.Flex;
			}
			else
			{
				_serviceListView.style.display = DisplayStyle.None;
			}

			_serviceListView.itemsSource = serviceList;
		}



		/// <summary> 서비스 목록 항목을 UI 요소와 바인딩합니다. </summary>
		/// <param name="element">서비스 데이터를 바인딩할 UI 요소</param>
		/// <param name="index">목록의 서비스 항목 인덱스</param>
		private void BindServiceItem(VisualElement element, int index)
		{
			if (element is not ServiceSection servicePanel || index < 0 || _serviceListView.itemsSource.Count <= index)
			{
				return;
			}

			ServiceBase service = (ServiceBase)_serviceListView.itemsSource[index];

			// Missing Object(스크립트 삭제)로 인해 null인 경우 스킵
			if (service == null)
			{
				servicePanel.style.display = DisplayStyle.None;
				return;
			}

			servicePanel.style.display = DisplayStyle.Flex;
			servicePanel.onDeleteRequested -= this.OnServiceDeletionRequested;
			servicePanel.onDeleteRequested += this.OnServiceDeletionRequested;

			servicePanel.Initialize(service);
		}



		/// <summary> 추가 서비스 버튼 클릭 이벤트 핸들러로, 서비스 선택 창을 호출합니다. </summary>
		/// <param name="evt">버튼 클릭 이벤트 정보</param>
		private void OnAddServiceButtonClicked(EventBase evt)
		{
			TaskGraphView view = TSEditor.Instance.taskGraphView;
			BindingWindow window = BindingWindowBuilder.GetBuilder("Services", reuse: true)
			                                           .AddFactoryModule(
				                                           () => new ServiceFactoryModule("Services", true),
				                                           () => new RelatedTypeTreeProvider(true))
			                                           .AddFactoryModule(
				                                           () => new ScriptCreationFactoryModule<CreateNewServiceScriptCommand>(view, "New Service"),
				                                           () => new RelatedTypeTreeProvider(false))
			                                           .Build();

			window.RegisterCreationCallbackOnce((Action<ServiceBase>)this.OnServiceCreated);
			window.OpenWindow(evt.originalMousePosition);
		}



		/// <summary>  새로 생성된 서비스를 리스트에 추가하고 패널을 갱신합니다. </summary>
		/// <param name="service">추가된 서비스 객체</param>
		private void OnServiceCreated(ServiceBase service)
		{
			Assert.IsNotNull(service, "service is null");
			_serviceListView.itemsSource.Add(service);
			this.RefreshPanel();
		}
		
		
		
		private VisualElement MakeItem() 
		{
			return new ServiceSection();
		}



		/// <summary> 서비스 삭제 요청 시 해당 서비스를 목록에서 제거합니다.  </summary>
		/// <param name="sectionPanel">삭제 요청이 발생한 서비스 섹션 패널</param>
		private void OnServiceDeletionRequested(ServiceSection sectionPanel)
		{
			if (_serviceListView.itemsSource.Count == 0)
			{
				return;
			}

			int index = _serviceListView.itemsSource.IndexOf(sectionPanel.service);

			if (index < 0 || index >= _serviceListView.itemsSource.Count)
			{
				return;
			}
			
			_serviceListView.itemsSource.RemoveAt(index);
			this.RefreshPanel();
		}
	}
}