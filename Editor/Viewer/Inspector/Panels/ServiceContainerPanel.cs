using System;
using System.Collections.Generic;
using TaskStreamer.BT;
using TaskStreamer.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class ServiceContainerPanel : VisualElement, IRefreshablePanel
    {
        public ServiceContainerPanel(VariableHandle servicesHandle)
        {
            TaskStreamerResourcesLoader.ServiceContainerPanel.CloneTree(this);

            _serviceList = servicesHandle.GetValue<List<ServiceBase>>();

            _elementContainer = this.Q<VisualElement>("container");
            _serviceListView = this.Q<ListView>("service-list");
            _addServiceButton = this.Q<Button>("add-button");

            _addServiceButton.clickable.clickedWithEventInfo += this.OnAddServiceButtonClick;
            
            this.RefreshPanel();

            _serviceListView.itemsSource = _serviceList;
            _serviceListView.bindItem += this.BindItem;
            _serviceListView.makeItem += () => new ServiceSectionPanel();
        }


        private readonly List<ServiceBase> _serviceList;

        private readonly ListView _serviceListView;

        private readonly Button _addServiceButton;

        private readonly VisualElement _elementContainer;

        

        public void RefreshPanel()
        {
            if (this._serviceListView.itemsSource is not null && this._serviceListView.itemsSource.Count != 0) 
            { 
                this._serviceListView.RefreshItems();
            }

            this._elementContainer.style.display = _serviceList.Count != 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }


        private void BindItem(VisualElement element, int index)
        {
            if (element is not ServiceSectionPanel fieldPanel)
            {
                return;
            }
            
            ServiceBase service = _serviceList[index];
            Type serviceType = service.GetType();

            string serviceName = StringUtility.ToNicifyName(serviceType.Name);
            List<VariableHandle> fields = TypeUtility.TryGetFieldHandles(serviceType, service);
            
            fieldPanel.Setup(serviceName, service, fields);
            
            fieldPanel.OnDeleteRequested -= this.FieldPanelOnOnDeleteRequested;
            fieldPanel.OnDeleteRequested += this.FieldPanelOnOnDeleteRequested;
        }

        
        
        /// 버튼 클릭 시 서비스 추가 창을 열어 사용자 액션을 처리하는 메서드입니다.
        /// <param name="evt">버튼 클릭과 관련된 이벤트 정보입니다.</param>
        private void OnAddServiceButtonClick(EventBase evt)
        {
            BindingWindow window = BindingWindowBuilder.GetBuilder("Services", false)
                                                       .AddFactoryModule(
                                                           () => new ServiceFactoryModule("Services", true),
                                                           () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<ServiceBase>)this.AddServiceToList);
            window.OpenWindow(evt.originalMousePosition);
        }



        private void AddServiceToList(ServiceBase service)
        {
            Debug.Assert(service is not null, "service is null");
            this._serviceList.Add(service);
            
            this.RefreshPanel();
        }
        
        
        
        private void FieldPanelOnOnDeleteRequested(ServiceSectionPanel sectionPanel)
        {
            if (_serviceListView.itemsSource is null || _serviceListView.itemsSource.Count == 0)
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