using System;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// BlackboardBasedCondition의 조건 목록을 UI로 표시하는 VisualElement 클래스.
    public class BBBasedConditionListField : VisualElement
    {
        public BBBasedConditionListField(VariableHandle fieldInfo)
        {
            TaskStreamerEditor.settings.bbBasedConditionListFieldXml.CloneTree(this);

            _conditionListView = this.Q<ListView>("condition-list-view");
            _conditionDeleteBtn = this.Q<Button>("condition-delete-btn");

            this.InitializeConditionListView(fieldInfo);
        }


        /// 조건 리스트를 표시하는 ListView UI 요소입니다.
        /// BBBasedConditionListField 클래스에서 BlackboardBasedCondition의 모듈을 시각화하여 관리하기 위해 사용됩니다.
        private readonly ListView _conditionListView;

        
        /// 조건 삭제 버튼을 나타내는 변수로, 조건 삭제와 관련된 이벤트 핸들링 연결에 사용됩니다.
        private readonly Button _conditionDeleteBtn;
        
        
        /// BlackboardBasedCondition 타입의 데이터를 저장하는 읽기 전용 변수입니다.
        private BlackboardBasedCondition _bbCondition;
        
        
        
        private void InitializeConditionListView(VariableHandle fieldInfo)
        {
            _bbCondition = fieldInfo.GetValue<BlackboardBasedCondition>();
            
            _conditionListView.headerTitle = StringUtility.ToNicifyName(fieldInfo.context);
            _conditionListView.itemsSource = _bbCondition!.modules;
            _conditionListView.bindItem = this.BindConditionItem;
            _conditionListView.makeItem = () => new BBBasedConditionField();

            _conditionDeleteBtn.clickable.clickedWithEventInfo -= this.OnAddButtonClicked;
            _conditionDeleteBtn.clickable.clickedWithEventInfo += this.OnAddButtonClicked;
        }
        


        /// 조건(CreationWindow)의 추가 버튼 클릭 시 수행되는 메서드입니다.
        /// <param name="evt">사용자가 추가 버튼 클릭 시 전달된 EventBase 객체입니다.</param>
        private void OnAddButtonClicked(EventBase evt)
        {
            ICreationWindow window = CreationWindow.GetCreationWindow("Conditions", false);

            if (window.modulesIsEmpty)
            {
                window.AddFactoryModule(new ConditionFactoryModule(typeof(Condition), "Conditions", 0));
            }

            window.RegisterCreationCallbackOnce((Action<Condition>)this.AddItemToList);
            window.OpenWindow(evt.originalMousePosition);
        }


        /// <summary>
        /// 주어진 VisualElement에 조건 데이터를 바인딩합니다.
        /// </summary>
        /// <param name="element">조건 데이터를 표시할 VisualElement.</param>
        /// <param name="index">바인딩할 데이터의 인덱스.</param>
        private void BindConditionItem(VisualElement element, int index)
        {
            BBBasedConditionField conditionField = element as BBBasedConditionField;

            Debug.Assert(conditionField is not null, "conditionField is null");
            
            conditionField.OnDeleteRequested -= this.OnVariableDeleteRequested;
            conditionField.OnDeleteRequested += this.OnVariableDeleteRequested;

            conditionField.Setup(this._bbCondition.modules[index]);
        }


        /// 리스트에 조건 아이템을 추가하는 메서드입니다. 추가 후 리스트를 새로고침하고 그래프 자산의 상태를 저장합니다.
        /// <param name="condition">추가할 조건 아이템입니다.</param>
        private void AddItemToList(Condition condition)
        {
            Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer (AddBBBasedCondition)");
            
            _conditionListView.itemsSource.Add(condition);
            _conditionListView.RefreshItems();

            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
        }


        /// 특정 변수가 삭제 요청되었을 때 처리하는 메서드입니다.
        /// <param name="variableView">삭제 요청된 BBBasedConditionField의 참조입니다.</param>
        private void OnVariableDeleteRequested(BBBasedConditionField variableView)
        {
            int index = _conditionListView.itemsSource.IndexOf(variableView.conditionValueValue);

            if (index < 0 || index >= _conditionListView.itemsSource.Count)
            {
                return;
            }
            
            Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer (RemoveBBBasedCondition)");
            
            _conditionListView.itemsSource.RemoveAt(index);
            _conditionListView.RefreshItems();
            
            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
        }
    }
}