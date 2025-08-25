using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ObjectFactory = TaskStreamer.Utility.ObjectFactory;

namespace TaskStreamer.Tool
{
    //BlackboardVariable 형태.
    /// <summary>Blackboard 변수의 필드를 UI로 나타내기 위한 클래스. TValue는 값의 타입, TBindableField는 값 바인딩에 사용하는 VisualElement 타입을 나타냅니다.</summary>
    public class BBVariableField<TValue, TBindableField> : VisualElement, INotifyValueChanged<TValue> where TBindableField : BindableElement, INotifyValueChanged<TValue>, new()
    {
        public BBVariableField(VariableHandle fieldInfo)
        {
            TaskStreamerEditor.settings.bbVariableFieldXml.CloneTree(this);

            _nameField = this.Q<Label>("name-field");
            _valueContainer = this.Q<VisualElement>("value-field");
            _contextSwapButton = this.Q<Toggle>("context-btn-field");

            this.InitializeVariableField(fieldInfo);
        }


        /// 경고 메시지를 표시할 때 사용하는 색상으로, 기본값은 노란색이다.
        private readonly Color _warningColor = Color.yellow;

        /// <summary>기본 색상으로 사용되며, 초기값은 흰색이다.</summary>
        private readonly Color _defaultColor = Color.white;

        /// <summary>변수의 값을 표시하거나 수정할 수 있는 UI 요소를 포함하는 컨테이너입니다.</summary>
        private readonly VisualElement _valueContainer;

        /// <summary>BlackboardVariable의 글로벌/로컬 상태 전환을 위한 context 변경 버튼입니다.</summary>
        private readonly Toggle _contextSwapButton;

        /// 변수의 이름을 표시하는 UI 요소를 나타냅니다.
        private readonly Label _nameField;

        /// <summary>Blackboard에서 선택 가능한 변수 이름들의 목록을 저장한다.</summary>
        private readonly List<string> _variableChoices = new List<string>();


        /// BlackBoard 변수에 대한 내부 데이터를 저장하는 필드이다.
        private BlackboardVariable _bbVariable;

        /// BlackboardVariable의 정보를 저장하고 조작하기 위한 변수입니다.
        private VariableHandle _fieldInfo;

        /// <summary>Blackboard 변수 값을 나타내는 필드로, UI 요소와 데이터 바인딩을 지원합니다.</summary>
        private TBindableField _variableField;

        /// <summary>Blackboard 변수의 드롭다운 필드를 나타내는 UI 요소입니다.</summary>
        private VisualElement _bbVariableDropdownField;


        /// <summary>BBVariableField에 바인딩된 값입니다.</summary>
        public TValue value
        {
            get;
            set;
        }

        /// UI 요소를 통해 값을 입력받거나 표시하기 위해 바인딩된 필드입니다.
        public TBindableField variableField
        {
            get { return _variableField; }
        }


        /// <summary> VariableHandle 정보를 이용하여 BBVariableField를 초기화합니다. </summary>
        /// <param name="fieldInfo">초기화에 사용하는 VariableHandle 정보입니다.</param>
        private void InitializeVariableField(VariableHandle fieldInfo)
        {
            _fieldInfo = fieldInfo;
            _bbVariable = fieldInfo.GetValue<BlackboardVariable>();
            Debug.Assert(_bbVariable is not null, "bbVariable is not null");
            
            bool needLabel = fieldInfo.context.IsNotNullOrEmpty();
            _nameField.style.display = needLabel ? DisplayStyle.Flex : DisplayStyle.None;
            _nameField.text = ObjectNames.NicifyVariableName(fieldInfo.context);

            this.RegisterVariableField(_bbVariable.isShared);

            _contextSwapButton.SetValueWithoutNotify(_bbVariable.isShared);
            _contextSwapButton.UnregisterValueChangedCallback(this.UsageContextChangeCallback);
            _contextSwapButton.RegisterValueChangedCallback(this.UsageContextChangeCallback);
        }


        /// <summary> 내부적으로 BlackboardVariable의 값을 설정합니다. </summary>
        /// <param name="variableValue">새롭게 설정할 변수 값입니다.</param>
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


        /// <summary> 지정된 변수를 필드로 등록하고 초기화를 수행합니다. </summary>
        private void RegisterVariableField(bool isShared)
        {
            VisualElement originalField = null;
            VisualElement newField = null;

            if (isShared)
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
                //Local <-> Global 전환이 가능하다보니, 전환됐을 때, 이전 Field는 삭제해야됨.
                this._valueContainer.Remove(originalField);
            }

            Debug.Assert(newField is not null, "newField is null");
            this._valueContainer.Add(newField);
        }


        /// <summary> 지역 변수 필드를 생성하고 초기화하여 반환하는 메서드입니다. </summary>
        /// <returns>생성된 지역 변수 필드 요소를 반환합니다.</returns>
        private TBindableField CreateLocalVariableField()
        {
            this._nameField.style.color = _defaultColor;
            this._variableField = new TBindableField();
            
            this._variableField.SetValueWithoutNotify((TValue)_bbVariable.boxedValue);
            
            this._variableField.UnregisterValueChangedCallback(this.VariableValueChangedCallback);
            this._variableField.RegisterValueChangedCallback(this.VariableValueChangedCallback);

            return this._variableField;
        }


        /// <summary> Blackboard에 정의된 전역 변수를 선택할 수 있는 Dropdown UI Element를 생성합니다. </summary>
        /// <returns>생성된 VisualElement Dropdown 필드입니다.</returns>
        private VisualElement CreateGlobalVariableField()
        {
            BlackboardAsset blackboard = TaskStreamerEditor.Instance.graphAsset?.blackboard;

            if (blackboard == null || blackboard.count == 0)
            {
                return this.GetEmptyDropdownField("No Variables");
            }

            BlackboardVariable[] bbVariables = null;

            if (_bbVariable.type is null)
            {
                Type generic = _bbVariable.GetType().GenericTypeArguments[0];
                Type variableType = typeof(BlackboardVariable<>).MakeGenericType(generic);
                bbVariables = blackboard.GetVariablesByType(variableType);
            }
            else
            {
                bbVariables = blackboard.GetVariablesByType(_bbVariable.type);
            }
            
            Debug.Assert(_bbVariable is not null, "_bbVariable is not null");
            BlackboardVariable foundVariable = _bbVariable is null ? null : blackboard.FindVariable(_bbVariable.guid);
            return this.GetVariableDropdownField(foundVariable is null ? "None" : foundVariable.key, bbVariables);
        }


        /// <summary> 빈 DropdownField를 생성합니다. </summary>
        /// <param name="message">DropdownField의 초기 메시지를 지정합니다.</param>
        /// <returns>경고 색상이 적용된 빈 DropdownField를 반환합니다.</returns>
        private VisualElement GetEmptyDropdownField(in string message)
        {
            DropdownField emptyDropdownField = new DropdownField();

            emptyDropdownField[0][0].style.color = _warningColor;
            emptyDropdownField.value = message;

            _nameField.style.color = _warningColor;
            return emptyDropdownField;
        }


        /// <summary> 주어진 변수 목록과 필드 이름을 바탕으로 DropdownField를 생성합니다. </summary>
        /// <param name="fieldName">DropdownField의 초기 선택 값으로 사용할 필드 이름입니다.</param>
        /// <param name="bbVariables">DropdownField에서 선택 가능한 Blackboard 변수들의 배열입니다.</param>
        /// <returns>DropdownField UI 요소를 반환합니다.</returns>
        private VisualElement GetVariableDropdownField(string fieldName, BlackboardVariable[] bbVariables)
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
        /// <summary> BlackboardVariable의 사용 컨텍스트(global/local)를 변경하기 위해 호출되는 콜백 함수입니다. </summary>
        /// <param name="evt">컨텍스트 변경 상태를 포함하는 이벤트입니다.</param>
        private void UsageContextChangeCallback(ChangeEvent<bool> evt)
        {
            Debug.Assert(_bbVariable != null, "Blackboard Variable is null");
            this._bbVariable.isShared = evt.newValue;

            if (evt.newValue) //sharedVariable
            {
                this.RegisterVariableField(true);
                return;
            }
            
            Type variableType = _bbVariable.type;

            if (variableType is null)
            {
                Type genericArg = _bbVariable.GetType().GenericTypeArguments[0];
                variableType = typeof(BlackboardVariable<>).GetImplementedType(genericArg);
            }

            string variableName = _nameField.text;
            object defaultValue = _fieldInfo.GetAttribute<SetValueAttribute>()?.defaultValue;
            _fieldInfo.SetValue(ObjectFactory.CreateBBVariable(variableType, variableName, defaultValue));
            this.RegisterVariableField(false);
        }


        /// <summary> 내부적으로 값을 변경하지만, 변경 알림은 발생시키지 않습니다. </summary>
        /// <param name="newValue">새롭게 설정할 값입니다.</param>
        public void SetValueWithoutNotify(TValue newValue)
        {
            this.SetVariableInternalValue(newValue);
        }


        /// <summary>  BlackboardVariable의 값이 변경되었을 때 호출되는 콜백 메서드입니다. </summary>
        /// <param name="evt">값 변경 이벤트로 새로운 값(newValue)이 포함됩니다.</param>
        private void VariableValueChangedCallback(ChangeEvent<TValue> evt)
        {
            this.SetVariableInternalValue(evt.newValue);
        }


        /// <summary> BlackboardVariable dropdown에서 값이 변경될 때 호출되는 콜백 함수입니다. </summary>
        /// <param name="evt">변경된 DropdownField의 이벤트 정보로, 새로운 값이 포함되어 있습니다.</param>
        private void OnChangeVariableCallback(ChangeEvent<string> evt)
        {
            if (TaskStreamerEditor.canEditGraph == false || TaskStreamerEditor.Instance.graphAsset?.blackboard == null)
            {
                return;
            }

            if (string.CompareOrdinal(evt.newValue, "None") == 0)
            {
                //TODO: 그냥 Null을 대입하고 Null도 가능하게 구현.
                string variableName = BlackboardVariable.DEFAULT_VARIABLE_NAME;
                var newVariable = ObjectFactory.CreateBBVariable(_bbVariable.type, variableName);
                newVariable.isShared = true;
                _fieldInfo.SetValue(newVariable);
                return;
            }
 
            BlackboardAsset blackboard = TaskStreamerEditor.Instance.graphAsset.blackboard; 
            BlackboardVariable selectedVariable = blackboard.FindVariable(evt.newValue); 
            Type type = typeof(SharedBlackboardVariable<>).MakeGenericType(_fieldInfo.fieldType.GenericTypeArguments[0]);
            _fieldInfo.SetValue(ObjectFactory.CreateSharedBBVariable(blackboard, selectedVariable.guid, type));
        }

#endregion
    }
}