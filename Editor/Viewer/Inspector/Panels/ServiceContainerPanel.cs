using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// ServiceContainerPanel 클래스는 VisualElement를 상속받아 서비스 리스트를 UI로 관리하며,
    /// IRefreshablePanel 인터페이스를 구현하여 동적 갱신 기능을 제공합니다.
    /// </summary>
    public class ServiceContainerPanel : VisualElement, IRefreshablePanel
    {
        /// <summary> 서비스 목록 패널을 나타내며, 서비스 항목을 추가 및 관리하는 UI 요소입니다. </summary>
        public ServiceContainerPanel(List<ServiceBase> servicesHandle, ObservableDictionary<ServiceBase, List<VariableHandle>> variableHandlesDic)
        {
            TaskStreamerResourcesLoader.ServiceContainerPanel.CloneTree(this);

            _variableHandles = variableHandlesDic;
            
            _elementContainer = this.Q<VisualElement>("container"); 
            _serviceListView = this.Q<ListView>("service-list"); 
            _addServiceButton = this.Q<Button>("add-button"); 
            
            _addServiceButton.enabledSelf = TaskStreamerEditor.canEditGraph;
            _addServiceButton.clickable.clickedWithEventInfo += this.OnAddServiceButtonClicked;
            _serviceListView.itemsSource = servicesHandle;
            _serviceListView.makeItem += () => new ServiceSectionPanel();
            _serviceListView.bindItem += BindServiceItem;

            this.RefreshPanel();
        }


        /// _variableHandles는 서비스의 고유 식별자인 UGUID와 해당 서비스의 VariableHandle 리스트를 연관 짓는
        /// ObservableDictionary로, 서비스별 변수 핸들을 관리합니다.
        private readonly ObservableDictionary<ServiceBase, List<VariableHandle>> _variableHandles;


        /// _serviceListView 변수는 서비스 목록을 표시하고 관리하기 위한 ListView UI 요소입니다.
        /// _serviceList 데이터를 바인딩하여 항목 표시 및 갱신 작업을 수행합니다.
        private readonly ListView _serviceListView;


        /// <summary> 새 서비스를 추가하는 버튼으로, 클릭 시 서비스 추가 로직이 실행됩니다. </summary>
        private readonly Button _addServiceButton;


        /// 현재 UI 컨테이너를 나타내는 VisualElement로, 서비스 목록 UI의 가시성을 제어하는 역할을 함.
        private readonly VisualElement _elementContainer;



        /// <summary> 서비스 목록과 UI 패널을 새로고침하여 현재 상태를 반영합니다. </summary>
        public void RefreshPanel()
        {
            if (_serviceListView.itemsSource is null)
            {
                Debug.LogError($"{typeof(ServiceContainerPanel)}'s itemsSource is null");
                return;
            }

            if (_serviceListView.itemsSource.Count > 0)
            {
                _serviceListView.RefreshItems();
                _elementContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                _elementContainer.style.display = DisplayStyle.None;
            }
        }



        /// <summary> 서비스 목록 항목을 UI 요소와 바인딩합니다. </summary>
        /// <param name="element">서비스 데이터를 바인딩할 UI 요소</param>
        /// <param name="index">목록의 서비스 항목 인덱스</param>
        private void BindServiceItem(VisualElement element, int index)
        {
            if (element is not ServiceSectionPanel servicePanel || index < 0 || _serviceListView.itemsSource.Count <= index)
            {
                return;
            }

            ServiceBase service = (ServiceBase)_serviceListView.itemsSource[index];

            servicePanel.OnDeleteRequested -= this.OnServiceDeletionRequested;
            servicePanel.OnDeleteRequested += this.OnServiceDeletionRequested;

            servicePanel.Initialize(service, _variableHandles[service]);
        }



        /// <summary> 추가 서비스 버튼 클릭 이벤트 핸들러로, 서비스 선택 창을 호출합니다. </summary>
        /// <param name="evt">버튼 클릭 이벤트 정보</param>
        private void OnAddServiceButtonClicked(EventBase evt)
        {
            BindingWindow window = CreateServiceBindingWindow();
            window.RegisterCreationCallbackOnce((Action<ServiceBase>)OnServiceCreated);
            window.OpenWindow(evt.originalMousePosition);
        }



        /// <summary> 서비스 바인딩을 위한 창을 생성합니다. </summary>
        /// <returns>생성된 BindingWindow 객체를 반환합니다.</returns>
        private BindingWindow CreateServiceBindingWindow()
        {
            return BindingWindowBuilder.GetBuilder("Services", false)
                                       .AddFactoryModule(
                                           () => new ServiceFactoryModule("Services", true),
                                           () => new TypeTreeProvider(true))
                                       .Build();
        }



        /// <summary>  새로 생성된 서비스를 리스트에 추가하고 패널을 갱신합니다. </summary>
        /// <param name="service">추가된 서비스 객체</param>
        private void OnServiceCreated(ServiceBase service)
        {
            Debug.Assert(service is not null, "service is null");

            List<VariableHandle> handles = TypeUtility.TryGetFieldHandles(service.GetType(), service);
            Debug.Assert(handles is not null, "handles is null");;
            
            _variableHandles.Add(service, handles);
            
            this.RefreshPanel();
        }



        /// <summary> 서비스 삭제 요청 시 해당 서비스를 목록에서 제거합니다.  </summary>
        /// <param name="sectionPanel">삭제 요청이 발생한 서비스 섹션 패널</param>
        private void OnServiceDeletionRequested(ServiceSectionPanel sectionPanel)
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

            this._variableHandles.Remove(sectionPanel.service);
            this.RefreshPanel();
        }
    }
}