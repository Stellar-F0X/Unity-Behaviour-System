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
    /// Blackboard 변수의 필드를 UI로 나타내기 위한 클래스. TValue는 값의 타입, TBindableField는 값 바인딩에 사용하는 VisualElement 타입을 나타냅니다.
    public class BBVariableField<TValue, TBindableField> : VisualElement, INotifyValueChanged<TValue> where TBindableField : BindableElement, INotifyValueChanged<TValue>, new()
    {
        /// BlackboardVariable에 대한 필드를 정의하는 제네릭 클래스.
        /// VisualElement를 상속하며, INotifyValueChanged 인터페이스를 구현.
        /// TValue는 값의 타입, TBindableField는 속성 바인딩에 사용되는 UI 요소 타입.
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

        /// 경고 메시지를 표시할 때 사용하는 색상으로, 기본값은 노란색이다.
        private readonly Color _warningColor = Color.yellow;

        /// 기본 변수 UI의 디폴트 색상을 나타냅니다. 초기 값은 Color.white입니다.
        private readonly Color _defaultColor = Color.white;

        /// SetValueAttribute 형식의 필드로, 변수의 초기화나 기본값 설정 시 활용됩니다.
        /// 등록된 필드에서 값을 설정하거나 처리하는 데 필요한 상태 정보를 저장합니다.
        private readonly SetValueAttribute _setValueAttribute;

        /// BlackboardVariable을 관리하기 위한 기본 저장소 역할을 하는 필드입니다.
        private readonly BlackboardVariable _bbVariable;

        /// Blackboard에서 선택 가능한 변수 이름들의 목록을 저장하는 변수.
        private readonly List<string> _variableChoices;

        /// _valueContainer는 변수의 시각적 구성 요소를 표시하는 UI 요소 컨테이너로,
        /// 새 필드를 추가하거나 기존 필드를 교체하는 역할을 수행합니다.
        private readonly VisualElement _valueContainer;

        /// BBVariableField UI에서 context 변경 버튼으로 사용됩니다.
        /// BlackboardVariable의 글로벌/로컬 상태를 전환합니다.
        private readonly Toggle _contextSwapButton;

        /// 사용자의 변수 이름을 표시하는 UI 요소를 나타냅니다.
        private readonly Label _nameField;

        /// BlackboardVariable의 바인딩된 값을 표시하고 관리하는 필드입니다.
        private TBindableField _variableField;

        /// 블랙보드 변수의 드롭다운 필드를 나타내는 UI 요소를 참조하는 변수.
        /// 로컬 및 글로벌 변수 선택에 사용되며, 관련 필드 생성 시 동적으로 초기화됨.
        private VisualElement _bbVariableDropdownField;


        /// 프로퍼티 값(TValue)을 가져오거나 설정합니다.
        public TValue value
        {
            get;
            set;
        }

        /// BBVariableField 클래스의 generic 필드로, 바인딩 가능한 UI 요소를 나타냅니다.
        /// TBindableField는 BindableElement와 INotifyValueChanged 인터페이스를 상속받아야 합니다.
        public TBindableField variableField
        {
            get { return _variableField; }
        }


        /// <summary>
        /// 내부적으로 BlackboardVariable의 값을 설정합니다.
        /// </summary>
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


        /// 지정된 변수를 필드로 등록하고 초기화 작업을 수행합니다.
        /// 필드의 구성 및 관리에 필요한 기본 로직을 포함합니다.
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
                //Local <-> Global 전환이 가능하다보니, 전환됐을 때, 이전 Field는 삭제해야됨.
                this._valueContainer.Remove(originalField);
            }

            Debug.Assert(newField is not null, "newField is null");
            this._valueContainer.Add(newField);
        }


        /// 지역 변수 필드를 생성하고 초기화하여 반환하는 메서드.
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


        /// Blackboard에 정의된 전역 변수를 선택할 수 있는 Dropdown UI Element를 생성한다.
        /// 변수가 없거나 사용 불가능한 경우 기본 필드로 대체된다.
        /// <returns>생성된 VisualElement Dropdown 필드</returns>
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


        /// 빈 DropdownField를 생성합니다.
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


        /// 주어진 변수 목록과 필드 이름을 바탕으로 DropdownField를 생성합니다.
        /// <param name="fieldName">DropdownField의 초기 선택 값으로 사용할 필드 이름입니다.</param>
        /// <param name="bbVariables">DropdownField에서 선택 가능한 Blackboard 변수들의 배열입니다.</param>
        /// <returns>DropdownField UI 요소를 반환합니다.</returns>
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

        /// BlackboardVariable의 사용 컨텍스트(global/local)를 변경하기 위해 호출되는 콜백 함수.
        /// <param name="evt">컨텍스트 변경 상태를 포함하는 이벤트</param>
        private void UsageContextChangeCallback(ChangeEvent<bool> evt)
        {
            Debug.Assert(_bbVariable != null, "Blackboard Variable is null");

            this._bbVariable.isGlobal = evt.newValue;

            if (evt.newValue) //isGlobal
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

            string variableName = _nameField.text;
            object defaultValue = _setValueAttribute?.defaultValue;

            this._bbVariable.variable = TSObjectFactory.CreateVariable(variableType, variableName, defaultValue);
            this.RegisterVariableField();
        }


        /// <summary>
        /// 내부적으로 값을 변경하지만, 변경 알림은 발생시키지 않습니다.
        /// </summary>
        /// <param name="newValue">새롭게 설정할 값입니다.</param>
        public void SetValueWithoutNotify(TValue newValue)
        {
            this.SetVariableInternalValue(newValue);
        }


        /// BlackboardVariable의 값이 변경되었을 때 호출되는 콜백 메서드.
        /// <param name="evt">값 변경 이벤트로 새로운 값(newValue)이 포함됩니다.</param>
        private void VariableValueChangedCallback(ChangeEvent<TValue> evt)
        {
            this.SetVariableInternalValue(evt.newValue);
        }


        /// BlackboardVariable dropdown에서 값이 변경될 때 호출되는 콜백 함수.
        /// <param name="evt">변경된 DropdownField의 이벤트 정보로, 새로운 값이 포함되어 있음.</param>
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