using System;
using System.Collections.Generic;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// BBBasedConditionField는 시각적 요소로, 조건 필드에 관련된 UI를 정의합니다.
    /// </summary>
    public class BlackBoardBasedConditionField : VisualElement
    {
        public BlackBoardBasedConditionField()
        {
            TaskStreamerEditor.settings.bbBasedConditionFieldXml.CloneTree(this);

            _leftVariableField = this.Q<VisualElement>("left--field-view--field");
            _rightVariableField = this.Q<VisualElement>("right--field-view--field");
            _comparisionField = this.Q<DropdownField>("comparison-type-field");
            _conditionDeleteButton = this.Q<Button>("condition-delete-btn");

            _conditionDeleteButton.UnregisterCallback<ClickEvent>(this.OnDeleteButtonClicked);
            _conditionDeleteButton.RegisterCallback<ClickEvent>(this.OnDeleteButtonClicked);

            _comparisionField.UnregisterValueChangedCallback(this.OnChangeComparisonType);
            _comparisionField.RegisterValueChangedCallback(this.OnChangeComparisonType);
        }


        /// <summary> 삭제 요청 이벤트를 발생시킵니다. </summary>
        public event Action<BlackBoardBasedConditionField> OnDeleteRequested;


        /// <summary> VisualElement that represents the left variable field in the condition UI. </summary>
        private readonly VisualElement _leftVariableField;


        /// <summary>사용자 데이터의 오른쪽 필드를 나타냅니다.</summary>
        private readonly VisualElement _rightVariableField;


        /// <summary> DropdownField representing the comparison type options. </summary>
        private readonly DropdownField _comparisionField;


        /// <summary> 버튼을 클릭하여 조건을 삭제하는 기능을 제공하는 버튼입니다. </summary>
        private readonly Button _conditionDeleteButton;


        /// <summary>조건 처리에 사용되는 값을 저장합니다.</summary>
        private Condition _conditionValue;



        /// <summary> 현재 조건의 값을 반환합니다. </summary>
        public Condition conditionValueValue
        {
            get { return _conditionValue; }
        }


        /// <summary> Condition 데이터를 기반으로 UI와 값을 초기화합니다. </summary>
        /// <param name="condition"> 초기화에 사용될 Condition 객체입니다. </param>
        public void Setup(Condition condition)
        {
            this._conditionValue = condition;
            this.tooltip = condition.tooltip;
            
            _leftVariableField.Clear();
            _rightVariableField.Clear();

            _leftVariableField.Add(VisualUtility.GetFieldByValueType(condition.GetLeftVariableHandle()));
            _rightVariableField.Add(VisualUtility.GetFieldByValueType(condition.GetRightVariableHandle()));

            this._comparisionField.value = _conditionValue.comparisonValue.ToString();
            
            StringUtility.TrySetNamesOfEnumFlag(condition.configuredComparisonType, this._comparisionField.choices);
        }


        /// <summary>비교 유형 변경 시 호출되는 로직을 처리합니다.</summary>
        /// <param name="evt">변경 이벤트 결과 객체입니다.</param>
        private void OnChangeComparisonType(ChangeEvent<string> evt)
        {
            int index = this._comparisionField.choices.IndexOf(evt.newValue);

            if (index < 0 || index >= this._comparisionField.choices.Count)
            {
                return;
            }

            Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer (ChangeBBVariableComparisonType)");

            _comparisionField.value = this._comparisionField.choices[index];
            
            //Comparison의 가장 첫 번째 값인 Comparison.EQ는 1부터 시작한다.
            _conditionValue.comparisonValue = (Comparison)(1 << index);

            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
        }


        /// <summary> 삭제 버튼이 클릭되었을 때 호출됩니다. </summary>
        /// <param name="evt"> 클릭 이벤트를 나타내는 ClickEvent 객체입니다. </param>
        private void OnDeleteButtonClicked(ClickEvent evt)
        {
            if (TaskStreamerEditor.canEditGraph == false || this._conditionValue is null)
            {
                return;
            }

            this.OnDeleteRequested?.Invoke(this);
        }
    }
}