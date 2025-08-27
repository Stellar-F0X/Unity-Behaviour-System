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
    public class BlackboardVariableField<TValue, TBindableField> : VisualElement, INotifyValueChanged<TValue> where TBindableField : BindableElement, INotifyValueChanged<TValue>, new()
    {
        /// <summary> BlackboardVariable을 바인딩할 수 있는 UI 요소를 제공합니다. </summary>
        public BlackboardVariableField(VariableHandle variableHandle)
        {
            Debug.Assert(variableHandle is not null, "bbVariable is not null");
            TaskStreamerEditor.settings.bbVariableFieldXml.CloneTree(this);

            this._variableNameLabel = this.Q<Label>("name-field");
            this._valueFieldContainer = this.Q<VisualElement>("value-field");
            this._unlinkButton = this.Q<Button>("unlink-button");
            this._linkToSharedButton = this.Q<Button>("link-button");

            this._variableHandle = variableHandle;
            this._blackboardVariable = variableHandle.GetValue<BlackboardVariable>();
            this._variableNameLabel.text = ObjectNames.NicifyVariableName(variableHandle.context);

            this._localVariableInputField = new TBindableField();
            this._localVariableInputField.SetValueWithoutNotify((TValue)_blackboardVariable.boxedValue);
            this._localVariableInputField.UnregisterValueChangedCallback(this.OnVariableValueChanged);
            this._localVariableInputField.RegisterValueChangedCallback(this.OnVariableValueChanged);

            this.SetupVariableField(this._blackboardVariable.isShared);

            this._unlinkButton.clickable.clicked -= this.OnConvertSharedToLocal;
            this._unlinkButton.clickable.clicked += this.OnConvertSharedToLocal;

            this._linkToSharedButton.clickable.clickedWithEventInfo -= this.OnOpenSharedVariableSelector;
            this._linkToSharedButton.clickable.clickedWithEventInfo += this.OnOpenSharedVariableSelector;
        }

#region Fields

        /// 경고 메시지에 사용되는 색상으로, 기본값은 노란색이다.
        private readonly Color _warningColor = Color.yellow;

        /// <summary>기본 색상으로 사용되며, 초기값은 흰색이다.</summary>
        private readonly Color _defaultColor = Color.white;

        /// <summary>공유되는 BlackboardVariable을 나타내는 색상입니다.</summary>
        private readonly Color _sharedVariableColor = new Color(0.1f, 0.85f, 1f, 1);

        /// <summary>변수의 값을 표시하거나 수정할 수 있는 UI 요소를 포함하는 컨테이너입니다.</summary>
        private readonly VisualElement _valueFieldContainer;

        /// <summary>공유 변수에서 로컬 변수로 변환하는 버튼.</summary>
        private readonly Button _unlinkButton;

        /// <summary>공유 변수 선택기를 열기 위한 버튼을 나타냅니다.</summary>
        private readonly Button _linkToSharedButton;

        /// 변수의 이름을 표시하는 UI 요소를 나타냅니다.
        private readonly Label _variableNameLabel;

        /// <summary>BlackBoard 변수에 대한 내부 데이터를 저장하는 필드이다.</summary>
        private BlackboardVariable _blackboardVariable;

        /// <summary>BlackboardVariable의 정보를 저장하고 조작하기 위한 변수입니다.</summary>
        private VariableHandle _variableHandle;

        /// <summary>Blackboard 변수 값을 UI에서 입력 및 바인딩할 수 있도록 처리하는 필드입니다.</summary>
        private TBindableField _localVariableInputField;

#endregion


        /// <summary>블랙보드 변수의 값을 나타내고 제어하는 프로퍼티입니다.</summary>
        public TValue value
        {
            get;
            set;
        }


        /// <summary>Blackboard 변수의 로컬 입력 필드를 나타냅니다.</summary>
        public TBindableField localVariableInputField
        {
            get { return _localVariableInputField; }
        }


        /// <summary> 공유 변수 선택기를 열고 설정을 진행합니다. </summary>
        /// <param name="eventArgs"> 공유 변수 선택기에 전달되는 이벤트 매개변수입니다. </param>
        private void OnOpenSharedVariableSelector(EventBase eventArgs)
        {
            Func<FactoryModule> moduleProvider = () => new SharedBBVariableFactoryModule("Shared Variables", true, 0);

            Func<ICategoryTreeProvider> categoryProvider = () => new SharedBBVariableProvider(_variableHandle.fieldType);

            BindingWindow window = BindingWindowBuilder.GetBuilder("Shared Variables", false)
                                                       .AddFactoryModule(moduleProvider, categoryProvider)
                                                       .TryUpdateModules()
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<ISharedBlackboardVariable>)this.OnConvertLocalToShared);
            window.OpenWindow(eventArgs.originalMousePosition);
        }


        /// <summary> 내부적으로 BlackboardVariable의 값을 설정합니다. </summary>
        /// <param name="newValue"> 새롭게 설정할 변수 값입니다. </param>
        private void UpdateBlackboardVariableValue(TValue newValue)
        {
            if (_blackboardVariable is BlackboardVariable<TValue> typedVariable)
            {
                Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer (ChangeBBVariableValue)");
                typedVariable.value = newValue;
                UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);
            }
            else
            {
                Debug.LogError($"Cannot cast the BlackboardVariable<{typeof(TValue).Name}>");
            }
        }


        /// <summary> 지정된 변수를 필드로 등록하고 초기화를 수행합니다. </summary>
        /// <param name="isSharedVariable"> 변수가 공유 변수인지 여부를 나타냅니다. </param>
        private void SetupVariableField(bool isSharedVariable)
        {
            switch (_blackboardVariable.usage)
            {
                case VariableUsage.Field: this._variableNameLabel.style.display = DisplayStyle.Flex; break;

                case VariableUsage.Condition: this._variableNameLabel.style.display = DisplayStyle.None; break;

                default: throw new ArgumentOutOfRangeException();
            }

            this._valueFieldContainer.Clear();

            if (isSharedVariable)
            {
                this._localVariableInputField.style.display = DisplayStyle.None;
                this._valueFieldContainer.Add(this.CreateSharedVariableDisplayField());
            }
            else
            {
                this._localVariableInputField.style.display = DisplayStyle.Flex;
                this._unlinkButton.style.display = DisplayStyle.None;
                this._variableNameLabel.style.color = _defaultColor;
                this._valueFieldContainer.Add(_localVariableInputField);
            }
        }


        /// <summary> Blackboard에 정의된 전역 변수를 선택할 수 있는 Dropdown UI Element를 생성합니다. </summary>
        /// <returns> 생성된 VisualElement Dropdown 필드입니다. </returns>
        private VisualElement CreateSharedVariableDisplayField()
        {
            BlackboardAsset blackboardAsset = TaskStreamerEditor.Instance.graphAsset?.blackboard;
            Debug.Assert(blackboardAsset != null, "blackboardAsset is null");

            Label displayLabel = new Label();
            displayLabel.style.letterSpacing = 2f;

            if (blackboardAsset == null || blackboardAsset.count == 0 || _blackboardVariable is null)
            {
                _variableHandle.SetValue(null);
                displayLabel.text = "Missing";
                displayLabel.style.color = _warningColor;
                _variableNameLabel.style.color = _warningColor;
                return displayLabel;
            }

            _unlinkButton.style.display = DisplayStyle.Flex;
            _variableNameLabel.style.color = _sharedVariableColor;
            displayLabel.text = _blackboardVariable.key;
            return displayLabel;
        }


#region Value Change Callbacks

        /// <summary> 내부적으로 값을 변경하지만, 변경 알림은 발생시키지 않습니다. </summary>
        /// <param name="newValue">새롭게 설정할 값입니다.</param>
        public void SetValueWithoutNotify(TValue newValue)
        {
            this.UpdateBlackboardVariableValue(newValue);
        }


        /// <summary>BlackboardVariable의 값이 변경되었을 때 호출되는 콜백 메서드입니다.</summary>
        /// <param name="changeEvent">값 변경 이벤트로 새로운 값(newValue)이 포함됩니다.</param>
        private void OnVariableValueChanged(ChangeEvent<TValue> changeEvent)
        {
            this.UpdateBlackboardVariableValue(changeEvent.newValue);
        }


        /// <summary> 로컬 변수를 SharedBlackboardVariable로 변환합니다. </summary>
        /// <param name="sharedVariable"> 변환될 SharedBlackboardVariable입니다. </param>
        private void OnConvertLocalToShared(ISharedBlackboardVariable sharedVariable)
        {
            Debug.Assert(sharedVariable is not null, "variable is null");

            if (sharedVariable is not BlackboardVariable variable)
            {
                Debug.LogError($"Cannot cast the BlackboardVariable<{typeof(TValue).Name}>");
                return;
            }
            
            variable.usage = _blackboardVariable.usage;
            this._variableHandle.SetValue(sharedVariable);

            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);

            this._blackboardVariable = variable;
            this.SetupVariableField(true);
        }


        /// <summary> BlackboardVariable를 Local로 변환합니다. </summary>
        private void OnConvertSharedToLocal()
        {
            BlackboardVariable newLocalVariable = ObjectFactory.CreateBlackboardVariable(_variableHandle.fieldType);
            Debug.Assert(newLocalVariable is not null, "newVariable is null");

            newLocalVariable.usage = _blackboardVariable.usage;
            this._variableHandle.SetValue(newLocalVariable);

            UnityEditor.EditorUtility.SetDirty(TaskStreamerEditor.Instance.graphAsset);

            this._blackboardVariable = newLocalVariable;
            this.SetupVariableField(false);
        }

#endregion
    }
}