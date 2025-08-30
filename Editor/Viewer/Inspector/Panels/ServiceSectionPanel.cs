using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.BT;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class ServiceSectionPanel : VisualElement, IRefreshablePanel
    {
        public ServiceSectionPanel()
        {
            TaskStreamerResourcesLoader.ServiceSectionPanel.CloneTree(this);

            _headerFoldout = this.Q<Foldout>("main-header");
            _fieldListView = this.Q<ListView>("field-list");
            _enableToggle = this.Q<Toggle>("enable-toggle");
            _deleteButton = this.Q<Button>("delete-button");

            _fieldListView.makeItem += () => new VisualElement();
            _fieldListView.bindItem += BindItem;
            _deleteButton.clicked += OnDeleteButtonClicked;

            _enableToggle.RegisterValueChangedCallback(evt => service.enable = evt.newValue);
            _headerFoldout.RegisterValueChangedCallback(evt => service.isExpanded = evt.newValue);
        }

        private readonly Foldout _headerFoldout;
        private readonly ListView _fieldListView;
        private readonly Toggle _enableToggle;
        private readonly Button _deleteButton;
        
        public event Action<ServiceSectionPanel> OnDeleteRequested;
        
        private List<VariableHandle> _fieldProperties;
        private ServiceBase _service;

        
        
        public ServiceBase service
        {
            get { return _service; }
        }



        private void OnDeleteButtonClicked()
        {
            if (TaskStreamerEditor.canEditGraph == false || _service is null)
            {
                return;
            }

            OnDeleteRequested?.Invoke(this);
        }

        
        
        public void RefreshPanel()
        {
            _fieldListView.RefreshItems();
        }

        
        
        public void Setup(string newName, ServiceBase newService, List<VariableHandle> fieldProperties)
        {
            _service = newService;
            _headerFoldout.text = newName;
            _fieldProperties = fieldProperties;
            _enableToggle.value = newService.enable;
            _headerFoldout.value = newService.isExpanded;
            _fieldListView.itemsSource = _fieldProperties;
        }

        
        
        private void BindItem(VisualElement element, int index)
        {
            VariableHandle handle = _fieldProperties[index];

            if (element.childCount == 0)
            {
                element.Add(this.CreateFieldElement(handle));
            }

            if (element.childCount > 0 && element[0] is IRefreshableField fieldRefreshable)
            {
                fieldRefreshable.RefreshVariableFieldPanel(handle);
            }
        }

        
        
        private VisualElement CreateFieldElement(VariableHandle handle)
        {
            switch (handle.value)
            {
                case BlackboardVariable: return VisualUtility.GetFieldByValueType(handle);

                case BlackboardBasedCondition: return new BlackboardBasedConditionListField(handle);

                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}