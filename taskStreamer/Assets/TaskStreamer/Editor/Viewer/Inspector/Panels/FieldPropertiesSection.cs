using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    internal class FieldPropertiesSection : VisualElement, IRefreshablePanel
    {
        public FieldPropertiesSection(List<VariableHandle> variableHandles)
        {
            TaskStreamerResourceLoader.fieldPropertiesSection.CloneTree(this);

            this._headerLabel = this.Q<Label>("main-header");
            this._fieldListView = this.Q<ListView>("field-list");

            this.RefreshPanelWithNewValue(variableHandles);

            this._fieldListView.makeItem += () => new VisualElement();
            this._fieldListView.bindItem += this.BindItem;
        }


        private List<VariableHandle> _variableHandles;

        private readonly ListView _fieldListView;
        
        //Using() {} 구문과 비슷한 개념으로, RefreshPanelWithNewValue 함수가 호출되는 동안 강제로 값을 갱신하도록 설정한다.
        private bool _forceRefreshWithValue;
        
        private Label _headerLabel;


        public void RefreshPanel()
        {
            if (this._fieldListView.itemsSource is null || this._fieldListView.itemsSource.Count == 0)
            {
                return;
            }

            this._forceRefreshWithValue = false;
            this._fieldListView.RefreshItems();
            this._forceRefreshWithValue = false;
        }



        public void RefreshPanelWithNewValue(object newValue)
        {
            Assert.IsNotNull(newValue, "New Value is null");
            List<VariableHandle> handles = newValue as List<VariableHandle>;
            Assert.IsNotNull(handles, $"New value is not a {typeof(List<VariableHandle>)}");

            this._variableHandles = handles;
            this._forceRefreshWithValue = true;
            this._fieldListView.itemsSource = handles;
            this._forceRefreshWithValue = false;
        }



        private void BindItem(VisualElement element, int index)
        {
            VariableHandle handle = this._variableHandles[index];

            if (this._forceRefreshWithValue == false && element.childCount != 0)
            {
                return;
            }

            if (this.TryCreateFieldElement(handle, out VisualElement field))
            {
                element.Clear();
                element.Add(field);
            }
        }


        private bool TryCreateFieldElement(VariableHandle handle, out VisualElement field)
        {
            switch (handle.initialValue)
            {
                case BlackboardVariable: field = VisualUtility.GetFieldByValueType(handle); break;

                case BlackboardBasedCondition: field = new ConditionListField(handle); break;

                //불필요한 필드들은 무시한다. 예를 들어, 빌드 전 에디터 환경에서 List<Transition>이나 List<ServiceBase>는 'Handle'로써 필요하지 않다.
                //여기서 필요한 것은 각각의 List로서 Transition이나 ServiceBase가 아니라, 그 객체들의 필드들인데, 이미 Handle로써 주어졌기 때문.
                //그리고 그 객체들은 플레이 모드 진입할때, ReadableVisitorBase에서 초기화 과정에서만 사용된다. 여기선 사용되지 않는다.
                default: field = null; break;
            }

            return field != null;
        }
    }
}