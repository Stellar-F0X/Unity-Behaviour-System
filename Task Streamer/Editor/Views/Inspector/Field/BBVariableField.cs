using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    //BlackboardVariable 형태.
    public class BBVariableField<TValue, TBindableField> : VisualElement, INotifyValueChanged<TValue> where TBindableField : BindableElement, INotifyValueChanged<TValue>, new()
    {
        public BBVariableField(string context, BlackboardVariable bbVariable, SetValueAttribute setValueAttribute)
        {
            TaskStreamerEditor.settings.bbVariableFieldXml.CloneTree(this);

            _nameField = this.Q<Label>("name-field");
            _valueContainer = this.Q<VisualElement>("value-field");
            _contextSwapButton = this.Q<Toggle>("context-btn-field");

            _nameField.style.display = context.IsNotNullOrEmpty() ? DisplayStyle.Flex : DisplayStyle.None;
            _nameField.text = ObjectNames.NicifyVariableName(context);

            _variableChoices = new List<string>();
            _setValueAttribute = setValueAttribute;
            _bbVariable = bbVariable;

            this.RegisterVariableField();

            _contextSwapButton.SetValueWithoutNotify(bbVariable.isGlobal);
            _contextSwapButton.UnregisterValueChangedCallback(this.UsageContextChangeCallback);
            _contextSwapButton.RegisterValueChangedCallback(this.UsageContextChangeCallback);
        }

        private readonly Color _warningColor = Color.yellow;
        private readonly Color _defaultColor = Color.white;

        private readonly SetValueAttribute _setValueAttribute;
        private readonly BlackboardVariable _bbVariable;
        private readonly List<string> _variableChoices;

        private readonly VisualElement _valueContainer;
        private readonly Toggle _contextSwapButton;
        private readonly Label _nameField;

        private TBindableField _variableField;
        private VisualElement _bbVariableDropdownField;


        public TValue value
        {
            get;
            set;
        }

        public TBindableField variableField
        {
            get { return _variableField; }
        }


        private void SetVariableInternalValue(TValue variableValue)
        {
            if (_bbVariable is BlackboardVariable<TValue> bbVariable)
            {
                Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer (ChangeBBVariableValue)");
                
                bbVariable.value = variableValue;
                
                UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
            }
            else
            {
                Debug.LogError($"Cannot cast the BlackboardVariable<{typeof(TValue).Name}>");
            }
        }


        private void RegisterVariableField()
        {
            VisualElement originalField = null;
            VisualElement newField = null;

            if (this._bbVariable.isGlobal)
            {
                originalField = variableField;
                newField = this.CreateGlobalVariableField();
                _bbVariableDropdownField = newField;
            }
            else
            {
                originalField = _bbVariableDropdownField;
                newField = this.CreateLocalVariableField();
                this._variableField = (TBindableField)newField;
            }

            if (originalField is not null)
            {
                this._valueContainer.Remove(originalField);
            }

            Debug.Assert(newField is not null, "newField is null");
            this._valueContainer.Add(newField);
        }


        private TBindableField CreateLocalVariableField()
        {
            this._nameField.style.color = _defaultColor;
            this._variableField = new TBindableField();

            this._variableField.SetValueWithoutNotify((TValue)_bbVariable.boxedValue);

            this._variableField.UnregisterValueChangedCallback(this.VariableValueChangedCallback);
            this._variableField.RegisterValueChangedCallback(this.VariableValueChangedCallback);

            return this._variableField;
        }


        private VisualElement CreateGlobalVariableField()
        {
            BlackboardAsset blackboard = TaskStreamerEditor.Instance.graphAsset?.blackboard;

            if (blackboard == null || blackboard.count == 0)
            {
                return this.GetEmptyDropdownField("No Variables");
            }

            Variable[] bbVariables = null;

            if (_bbVariable.type is null)
            {
                Type generic = _bbVariable.GetType().GenericTypeArguments[0];
                Type variableType = typeof(Variable<>).MakeGenericType(generic);
                bbVariables = blackboard.GetVariablesByType(variableType);
            }
            else
            {
                bbVariables = blackboard.GetVariablesByType(_bbVariable.type);
            }

            Variable foundVariable = _bbVariable.variable is null ? null : blackboard.FindVariable(_bbVariable.guid);

            return this.GetVariableDropdownField(foundVariable is null ? "None" : foundVariable.key, bbVariables);
        }


        private VisualElement GetEmptyDropdownField(in string message)
        {
            DropdownField emptyDropdownField = new DropdownField();

            emptyDropdownField[0][0].style.color = _warningColor;
            emptyDropdownField.value = message;

            _nameField.style.color = _warningColor;
            return emptyDropdownField;
        }


        private VisualElement GetVariableDropdownField(string fieldName, Variable[] bbVariables)
        {
            DropdownField variableSelectionField = new DropdownField();

            _variableChoices.Clear();
            _variableChoices.Add("None");
            _variableChoices.AddRange(bbVariables.Select(v => v.key));

            variableSelectionField.value = fieldName;
            variableSelectionField.choices = _variableChoices;

            variableSelectionField.UnregisterValueChangedCallback(this.OnChangeVariableCallback);
            variableSelectionField.RegisterValueChangedCallback(this.OnChangeVariableCallback);

            return variableSelectionField;
        }


#region Value Change Callbacks
        private void UsageContextChangeCallback(ChangeEvent<bool> evt)
        {
            Debug.Assert(_bbVariable != null, "Blackboard Variable is null");

            this._bbVariable.isGlobal = evt.newValue;

            if (evt.newValue)
            {
                this.RegisterVariableField();
                return;
            }

            Type variableType = _bbVariable.type;

            if (variableType is null)
            {
                Type genericArg = _bbVariable.GetType().GenericTypeArguments[0];
                variableType = typeof(Variable<>).GetImplementedType(genericArg);
            }

            this._bbVariable.variable = TSObjectFactory.CreateVariable(variableType, _setValueAttribute?.defaultValue);
            this.RegisterVariableField();
        }


        public void SetValueWithoutNotify(TValue newValue)
        {
            this.SetVariableInternalValue(newValue);
        }


        private void VariableValueChangedCallback(ChangeEvent<TValue> evt)
        {
            this.SetVariableInternalValue(evt.newValue);
        }


        private void OnChangeVariableCallback(ChangeEvent<string> evt)
        {
            if (TaskStreamerEditor.canEditGraph == false || TaskStreamerEditor.Instance.graphAsset?.blackboard == null)
            {
                return;
            }

            if (string.CompareOrdinal(evt.newValue, "None") == 0)
            {
                _bbVariable.variable = null;
                return;
            }

            BlackboardAsset blackboard = TaskStreamerEditor.Instance.graphAsset.blackboard;
            Variable selectedVariable = blackboard.FindVariable(evt.newValue);
            _bbVariable.variable = selectedVariable;
        }

#endregion
    }
}