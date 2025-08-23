using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class BBBasedConditionField : VisualElement
    {
        public BBBasedConditionField()
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

        public event Action<BBBasedConditionField> OnDeleteRequested;

        private readonly VisualElement _leftVariableField;
        private readonly VisualElement _rightVariableField;
        private readonly DropdownField _comparisionField;
        private readonly Button _conditionDeleteButton;

        private Condition _conditionValue;


        public Condition conditionValueValue
        {
            get { return _conditionValue; }
        }



        public void Setup(Condition condition)
        {
            this._conditionValue = condition;
            this.tooltip = condition.tooltip;

            BlackboardVariable leftVariable = condition.encapsulatedLeftVariable;
            BlackboardVariable rightVariable = condition.encapsulatedRightVariable;

            _leftVariableField.Clear();
            _rightVariableField.Clear();

            _leftVariableField.Add(VisualUtility.GetFieldByValueType(string.Empty, leftVariable));
            _rightVariableField.Add(VisualUtility.GetFieldByValueType(string.Empty, rightVariable));
            
            this._comparisionField.value = _conditionValue.comparisonValue.ToString();
            this.TrySetComparisonTypeNames(condition.configuredComparisonType, this._comparisionField.choices);
        }


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


        private void OnDeleteButtonClicked(ClickEvent evt)
        {
            if (TaskStreamerEditor.canEditGraph == false || this._conditionValue is null)
            {
                return;
            }

            this.OnDeleteRequested?.Invoke(this);
        }


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