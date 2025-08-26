using System;
using System.Collections.Generic;
using TaskStreamer.Injection;
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

            BlackboardVariable leftVariable = condition.encapsulatedLeftVariable;
            BlackboardVariable rightVariable = condition.encapsulatedRightVariable;

            _leftVariableField.Clear();
            _rightVariableField.Clear();

            Func<Condition, object> getLeft = c => c.encapsulatedLeftVariable;
            Func<Condition, object> getRight = c => c.encapsulatedRightVariable;

            Action<Condition, object> setLeft = (c, v) => c.encapsulatedLeftVariable = v as BlackboardVariable;
            Action<Condition, object> setRight = (c, v) => c.encapsulatedRightVariable = v as BlackboardVariable;

            VariableHandle rightHandle = new VariableHandle("", leftVariable, condition, leftVariable.implementedType.BaseType, null, getLeft, setLeft);
            VariableHandle leftHandle = new VariableHandle("", rightVariable, condition, rightVariable.implementedType.BaseType, null, getRight, setRight);

            _leftVariableField.Add(VisualUtility.GetFieldByValueType(rightHandle));
            _rightVariableField.Add(VisualUtility.GetFieldByValueType(leftHandle));

            this._comparisionField.value = _conditionValue.comparisonValue.ToString();
            this.TrySetComparisonTypeNames(condition.configuredComparisonType, this._comparisionField.choices);
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
            _conditionValue.comparisonValue = (Comparison)(index == 0 ? 0 : 1 << (index - 1));

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


        /// <summary> 주어진 비교 타입과 이름 목록을 기반으로 비교 타입 이름을 설정합니다. </summary>
        /// <param name="comparison">적용할 비교 타입 플래그입니다.</param>
        /// <param name="names">비교 타입 이름 목록입니다.</param>
        private void TrySetComparisonTypeNames(Comparison comparison, List<string> names)
        {
            if (names is null)
            {
                names = new List<string>();
            }

            if (names.Count != 0)
            {
                return;
            }

            names.Add("None");

            for (int index = (int)Comparison.EQ; index <= (int)Comparison.LE; index <<= 1)
            {
                if (((Comparison)index & comparison) != Comparison.None)
                {
                    names.Add(((Comparison)index).ToString());
                }
            }
        }
    }
}