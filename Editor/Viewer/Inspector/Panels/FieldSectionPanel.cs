using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    internal class FieldSectionPanel : VisualElement, IRefreshablePanel
    {
        public FieldSectionPanel(List<VariableHandle> variableHandles)
        {
            TaskStreamerResourcesLoader.FieldSectionPanel.CloneTree(this);

            headerLabel = this.Q<Label>("main-header");
            _fieldListView = this.Q<ListView>("field-list");

            _variableHandles = variableHandles;
            _fieldListView.itemsSource = _variableHandles;
            _fieldListView.makeItem += () => new VisualElement();
            _fieldListView.bindItem += this.BindItem;
        }


        private readonly List<VariableHandle> _variableHandles;

        private readonly ListView _fieldListView;

        
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
            
            _fieldListView.RefreshItems();
        }


        private void BindItem(VisualElement element, int index)
        {
            VariableHandle handle = _variableHandles[index];

            if (element.childCount == 0)
            {
                element.Add(CreateFieldElement(handle));
            }

            if (element.childCount > 0 && element[0] is IRefreshableField fieldRefreshable)
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