using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    //BlackboardVariable 형태.
    /// <summary>Blackboard 변수의 필드를 UI로 나타내기 위한 클래스. TValue는 값의 타입, TBindableField는 값 바인딩에 사용하는 VisualElement 타입을 나타냅니다.</summary>
    internal class BlackboardVariableField<TValue, TBindableField> : VisualElement, INotifyValueChanged<TValue>, IRefreshableField where TBindableField : BindableElement, INotifyValueChanged<TValue>, new()
    {
        protected BlackboardVariableField()
        {
            TSEditor.bbVariableField.CloneTree(this);
            
            this._variableNameLabel = this.Q<Label>("name-field");
            this._valueFieldContainer = this.Q<VisualElement>("value-field");
            this._unlinkButton = this.Q<Button>("unlink-button");
            this._linkToSharedButton = this.Q<Button>("link-button");
        }
        
        
        public BlackboardVariableField(VariableHandle variableHandle) : this()
        {
            Assert.IsNotNull(variableHandle, "bbVariable is not null");

            this._variableHandle = variableHandle;
            this._blackboardVariable = variableHandle.GetValue<BlackboardVariable>();
            Assert.IsNotNull(this._blackboardVariable, "Blackboard Variable is unavailable");

            this._variableNameLabel.text = ObjectNames.NicifyVariableName(variableHandle.context);

            this._localVariableInputField = new TBindableField();
            this._localVariableInputField.RegisterValueChangedCallback(this.OnVariableValueChanged);
            
            this.CreateVariableFieldByType(this._blackboardVariable.isShared);

            this._unlinkButton.clickable.clicked += this.OnConvertSharedToLocal;
            this._unlinkButton.iconImage = TSEditor.deleteButton;
            this._unlinkButton.enabledSelf = TSEditor.canEditGraph && TSEditor.hasBlackboard;

            this._linkToSharedButton.clickable.clickedWithEventInfo += this.OnOpenSharedVariableSelector;
            this._linkToSharedButton.iconImage = TSEditor.bindingButton;
            this._linkToSharedButton.enabledSelf = TSEditor.canEditGraph && TSEditor.hasBlackboard;
        }


#region Fields

        private const int _NAME_UPDATE_INTERVAL = 250;


        /// <summary>기본 색상으로 사용되며, 초기값은 흰색이다.</summary>
        private readonly Color _defaultVariableLabelColor = Color.white;


        /// <summary>공유되는 BlackboardVariable을 나타내는 색상입니다.</summary>
        private readonly Color _sharedVariableLabelColor = new Color(0.2f, 0.9f, 1f);


        /// <summary>변수 값을 표시하거나 수정하는 UI 요소가 포함된 컨테이너입니다.</summary>
        private readonly VisualElement _valueFieldContainer;


        /// <summary>공유 변수에서 로컬 변수로 변환하는 버튼.</summary>
        private readonly Button _unlinkButton;


        /// <summary>공유 변수 선택기를 열기 위한 버튼을 나타냅니다.</summary>
        private readonly Button _linkToSharedButton;


        /// <summary>변수 이름을 표시하는 UI 요소입니다.</summary>
        private readonly Label _variableNameLabel;


        /// <summary>Blackboard 변수에 대한 내부 데이터를 저장하는 필드이다.</summary>
        protected BlackboardVariable _blackboardVariable;


        /// <summary>BlackboardVariable의 정보를 저장하고 조작하기 위한 변수입니다.</summary>
        private VariableHandle _variableHandle;


        /// <summary>Blackboard 변수 입력값 처리를 위한 바인딩 가능한 필드입니다.</summary>
        private TBindableField _localVariableInputField;

#endregion

        
        [Obsolete("BlackboardVariable은 직접 값을 대입하는 방식으로 사용되지 않습니다.")]
        public TValue value
        {
            get;
            set;
        }


        /// <summary>로컬 변수 값을 나타내거나 수정하기 위한 입력 필드입니다.</summary>
        public TBindableField localVariableInputField
        {
            get { return _localVariableInputField; }
        }



        public void RefreshVariableFieldPanel(VariableHandle handle)
        {
            BlackboardVariable variable = handle.GetValue<BlackboardVariable>();
            Assert.IsNotNull(variable, "variable is null");

            this._blackboardVariable = variable;

            this.CreateVariableFieldByType(variable.isShared);
        }


        /// <summary> Blackboard 변수의 값을 갱신합니다. </summary>
        /// <param name="newValue"> 새롭게 설정할 변수 값입니다. </param>
        protected virtual void UpdateBlackboardVariableValue(TValue newValue)
        {
            BlackboardVariable<TValue> typedVariable = _blackboardVariable as BlackboardVariable<TValue>;
            Type impType = _blackboardVariable.genericVariableType;
            Assert.IsNotNull(typedVariable, $"{impType} is Cannot cast the BlackboardVariable<{typeof(TValue).Name}>");

            Undo.RecordObject(TSEditor.Instance.graphAsset, "TaskStreamer (ChangeBBVariableValue)");
            typedVariable.value = newValue;
            UnityEditor.EditorUtility.SetDirty(TSEditor.Instance.graphAsset);
        }


        /// <summary> 변수의 공유 상태에 따라 필드를 설정하고 초기화합니다. </summary>
        /// <param name="isSharedVariable"> 공유 변수 여부를 나타냅니다. </param>
        private void CreateVariableFieldByType(bool isSharedVariable)
        {
            switch (_blackboardVariable.usage)
            {
                case VariableUsage.Field: this._variableNameLabel.style.display = DisplayStyle.Flex; break;

                case VariableUsage.Condition: this._variableNameLabel.style.display = DisplayStyle.None; break;

                default: Debug.LogError($"Unknown VariableUsage: {_blackboardVariable.usage}"); break;
            }

            if (this._valueFieldContainer.childCount > 0)
            {
                this._valueFieldContainer.Clear();
            }

            if (isSharedVariable)
            {
                this._localVariableInputField.style.display = DisplayStyle.None;
                this._valueFieldContainer.Add(this.CreateSharedVariableDisplayField());
            }
            else
            {
                this._localVariableInputField.style.display = DisplayStyle.Flex;
                this._valueFieldContainer.Add(this.CreateLocalVariableDisplayField());
            }

            ReadOnlyAttribute readOnly = this._variableHandle.GetAttribute<ReadOnlyAttribute>(); 

            //여기서 결정해도 InitializeBlackboardVariableField 함수에서 런타임 중인지 아닌지에 따라 활성화가 다시 결정된다.
            this._localVariableInputField.enabledSelf = readOnly is null;
            this._linkToSharedButton.enabledSelf = readOnly is null || !TSEditor.canEditGraph;
        }


        /// <summary> 내부적으로 값을 변경하지만, 변경 알림은 발생시키지 않습니다. </summary>
        /// <param name="newValue">새롭게 설정할 값입니다.</param>
        public void SetValueWithoutNotify(TValue newValue)
        {
            this.UpdateBlackboardVariableValue(newValue);
        }


        /// <summary> BlackboardVariable의 값이 변경되었을 때 호출되는 콜백 메서드입니다. </summary>
        /// <param name="changeEvent"> 값 변경 이벤트로 새로운 값(newValue)이 포함됩니다. </param>
        private void OnVariableValueChanged(ChangeEvent<TValue> changeEvent)
        {
            this.UpdateBlackboardVariableValue(changeEvent.newValue);
        }



#region Shared Variable

        /// <summary> 공유 변수 선택기를 열고 설정을 진행합니다. </summary>
        /// <param name="eventArgs"> 공유 변수 선택기에 전달되는 이벤트 매개변수입니다. </param>
        private void OnOpenSharedVariableSelector(EventBase eventArgs)
        {
            BindingWindow window = BindingWindowBuilder.GetBuilder("Shared Variables", false)
                                                       .AddFactoryModule(
                                                           () => new SharedBlackboardVariableFactoryModule("Shared Variables", true, 0),
                                                           () => new SharedBBVariableProvider(_variableHandle.fieldType))
                                                       .TryUpdateModules()
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<ISharedBlackboardVariable>)this.OnConvertLocalToShared);
            window.OpenWindow(eventArgs.originalMousePosition);
        }


        /// <summary> Blackboard에 정의된 전역 변수를 선택할 수 있는 Dropdown UI Element를 생성합니다. </summary>
        /// <returns> 생성된 VisualElement Dropdown 필드입니다. </returns>
        private VisualElement CreateSharedVariableDisplayField()
        {
            BlackboardAsset blackboardAsset = TSEditor.Instance.graphAsset?.blackboard;

            //이 경우 블랙보드가 제거되면서 필드에 등록된 BBVariable들도 모두 제거됐어야 정상이다.
            if (blackboardAsset == null || blackboardAsset.count == 0 || _blackboardVariable is null)
            {
                _variableHandle.SetValue(null);
                throw new ArgumentException("Blackboard Or Blackboard Variable is unavailable");
            }

            Label displayContentLabel = new Label(_blackboardVariable.key);
            displayContentLabel.style.letterSpacing = 2f;

            _variableNameLabel.style.color = _sharedVariableLabelColor;
            _unlinkButton.style.display = DisplayStyle.Flex;

            schedule.Execute(() => displayContentLabel.text = _blackboardVariable.key)
                    .Until(() => _blackboardVariable != null && _blackboardVariable.isShared && this.enabledInHierarchy)
                    .Every(_NAME_UPDATE_INTERVAL);

            return displayContentLabel;
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

            UnityEditor.EditorUtility.SetDirty(TSEditor.Instance.graphAsset);

            this._blackboardVariable = variable;
            this.CreateVariableFieldByType(true);
        }

#endregion



#region Local Variable

        /// <summary> 로컬 변수를 표시하기 위한 UI 필드를 생성합니다. </summary>
        /// <returns> 생성된 VisualElement입니다. </returns>
        private VisualElement CreateLocalVariableDisplayField()
        {
            Assert.IsNotNull(this._blackboardVariable, "Blackboard Variable is unavailable");

            this._unlinkButton.style.display = DisplayStyle.None;
            this._variableNameLabel.style.color = _defaultVariableLabelColor;

            bool assignable = typeof(TValue).IsAssignableFrom(this._blackboardVariable.valueType);
            Assert.IsTrue(assignable, $"type is not {typeof(TValue)}, That is {_blackboardVariable.valueType}");
            
            this._localVariableInputField.SetValueWithoutNotify((TValue)this._blackboardVariable.boxedValue);
            return this._localVariableInputField;
        }


        /// <summary> BlackboardVariable를 Local로 변환합니다. </summary>
        private void OnConvertSharedToLocal()
        {
            DefaultValueAttribute setValue = this._variableHandle.GetAttribute<DefaultValueAttribute>();

            BlackboardVariable newLocal;

            if (setValue is not null)
            {
                newLocal = TSObjectFactory.CreateBlackboardVariable(_variableHandle.fieldType, defaultValue: setValue.defaultValue);
            }
            else
            {
                newLocal = TSObjectFactory.CreateBlackboardVariable(_variableHandle.fieldType);
            }

            Assert.IsNotNull(newLocal, "newVariable is null");

            newLocal.usage = _blackboardVariable.usage;
            this._variableHandle.SetValue(newLocal);

            UnityEditor.EditorUtility.SetDirty(TSEditor.Instance.graphAsset);

            this._blackboardVariable = newLocal;
            this.CreateVariableFieldByType(false);
        }

#endregion
    }
}