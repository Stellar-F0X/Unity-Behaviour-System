using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    internal class FieldSectionPanel : VisualElement, IRefreshablePanel
    {
        public FieldSectionPanel(List<VariableHandle> variableHandles)
        {
            TaskStreamerResourceLoader.FieldSectionPanel.CloneTree(this);

            headerLabel = this.Q<Label>("main-header");
            _fieldListView = this.Q<ListView>("field-list");

            this.RefreshPanelWithNewValue(variableHandles);

            _fieldListView.makeItem += () => new VisualElement();
            _fieldListView.bindItem += this.BindItem;
        }


        private List<VariableHandle> _variableHandles;

        private readonly ListView _fieldListView;

        private bool _forceRefreshWithValue;


        public Label headerLabel
        {
            get;
            private set;
        }


        public void RefreshPanel()
        {
            if (_fieldListView.itemsSource is null || _fieldListView.itemsSource.Count == 0)
            {
                return;
            }

            _forceRefreshWithValue = false;
            _fieldListView.RefreshItems();
            _forceRefreshWithValue = false;
        }



        public void RefreshPanelWithNewValue(object newValue)
        {
            if (newValue is not List<VariableHandle> handles)
            {
                Debug.LogError("New value is not a List<VariableHandle>");
                return;
            }

            _variableHandles = handles;

            _forceRefreshWithValue = true;
            _fieldListView.itemsSource = handles;
            _forceRefreshWithValue = false;
        }



        private void BindItem(VisualElement element, int index)
        {
            VariableHandle handle = _variableHandles[index];

            if (this._forceRefreshWithValue || element.childCount == 0)
            {
                element.Clear();
                element.Add(this.CreateFieldElement(handle));
            }

            if (element[0] is IRefreshableField fieldRefreshable)
            {
                fieldRefreshable.RefreshVariableFieldPanel(handle);
            }
        }


        private VisualElement CreateFieldElement(VariableHandle handle)
        {
            switch (handle.initialValue)
            {
                case BlackboardVariable: return VisualUtility.GetFieldByValueType(handle);

                case BlackboardBasedCondition: return new BlackboardBasedConditionListField(handle);

                default: throw new System.ArgumentException($"Unsupported handle value type: {handle.initialValue?.GetType()}");
            }
        }
    }
}