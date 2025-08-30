using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// TaskStreamer 도구에서 속성 및 바인딩을 관리하기 위한 블랙보드 역할을 하는 ListView 클래스입니다.
    /// </summary>
    [UxmlElement]
    public partial class BlackboardView : ListView
    {
        /// <summary>
        /// SerializedProperty로 BlackboardAsset의 변수 목록 데이터를 관리하며, UI와 동기화에 활용됩니다.
        /// </summary>
        private SerializedProperty _serializedList;


        /// <summary>
        /// BlackboardAsset에 바인딩된 데이터의 직렬화 및 관리를 지원하기 위해 사용되는 SerializedObject 인스턴스.
        /// </summary>
        private SerializedObject _serializedObject;


        /// <summary>
        /// 현재 블랙보드 데이터 모델(<see cref="BlackboardAsset"/>)의 인스턴스를 저장하여 UI와 데이터 동기화를 관리합니다.
        /// </summary>
        private BlackboardAsset _blackboard;


        /// <summary>
        /// UI의 ObjectField로, BlackboardAsset을 바인딩해 사용자와 데이터 간 연결을 관리합니다.
        /// </summary>
        private ObjectField _blackboardBindingField;

        /// <summary>
        /// 새 변수를 추가하기 위한 버튼 UI 요소를 나타냅니다.
        /// 사용자가 클릭 이벤트를 통해 새 변수를 생성할 수 있도록 제공합니다.
        /// </summary>
        private Button _variableAddButton;


        
#region Public Methods

        /// <summary>블랙보드 뷰의 모든 정보를 제거하고 초기 상태로 복원합니다.</summary>
        public void ClearBlackboardView()
        {
            this.Clear();
            
            this.itemsSource = null;
            this._serializedList = null;
            this._serializedObject = null;
            
            this._blackboard = null;
            
            this._variableAddButton.clickable = null;
            this._blackboardBindingField.SetValueWithoutNotify(null);

            // 인스펙터도 함께 정리
            TaskStreamerEditor.Instance.inspectorView.ClearInspector();
            
            this.RefreshItems();
        }


        /// <summary>Setup 메서드는 블랙보드의 프로퍼티 리스트 뷰를 초기화하고 관련 UI 요소들을 바인딩합니다.</summary>
        /// <param name="variableAddButton">블랙보드에 변수를 추가하기 위한 버튼입니다.</param>
        /// <param name="blackboardBindingField">블랙보드와의 바인딩을 지원하는 오브젝트 필드입니다.</param>
        public void Setup(Button variableAddButton, ObjectField blackboardBindingField)
        {
            this._variableAddButton = variableAddButton;
            this._blackboardBindingField = blackboardBindingField;

            this.bindItem = this.BindItemToList;
            this.makeItem = () => new BlackboardVariableView();

            this.itemIndexChanged -= this.OnPropertyIndicesSwapped;
            this.itemIndexChanged += this.OnPropertyIndicesSwapped;

            this._variableAddButton.UnregisterCallback<ClickEvent>(this.OpenContextualMenuWindow);
            this._variableAddButton.RegisterCallback<ClickEvent>(this.OpenContextualMenuWindow);

            this._blackboardBindingField.UnregisterValueChangedCallback(this.OnBindBlackboardAsset);
            this._blackboardBindingField.RegisterValueChangedCallback(this.OnBindBlackboardAsset);
        }


        /// <summary>새로운 블랙보드 자산을 적용하거나 기존 블랙보드를 초기화합니다.</summary>
        /// <param name="newBlackboard">적용할 블랙보드 자산입니다. null을 전달하면 블랙보드 뷰를 초기화합니다.</param>
        /// <returns>블랙보드가 성공적으로 변경되면 true를, 실패하거나 동일한 경우 false를 반환합니다.</returns>
        public bool TryChangeBlackboard(BlackboardAsset newBlackboard)
        {
            //새롭게 들어온 블랙보드가 현재 블랙보드와 동일한 경우에는 아무 작업도 하지 않는다. 
            if (this._blackboard == newBlackboard)
            {
                return false;
            }

            this._blackboard = newBlackboard;
            this._blackboardBindingField.SetValueWithoutNotify(newBlackboard);
            TaskStreamerEditor.Instance.graphAsset.blackboard = _blackboard;

            if (newBlackboard is null)
            {
                this.ClearBlackboardView();
                return true;
            }

            if (this.InitializeBlackboard(newBlackboard) == false)
            {
                Debug.LogError("Serialized list property is null.");
                return false;
            }

            this.UpdateBlackboardView();
            return true;
        }


        /// <summary>Undo 작업이 수행될 때 블랙보드 뷰의 아이템을 새로고침합니다.</summary>
        /// <exception cref="Exception">블랙보드의 직렬화된 리스트 속성이 null일 경우 예외를 발생시킵니다.</exception>
        public void RefreshItemsWhenUndoPerformed()
        {
            this._blackboard = TaskStreamerEditor.Instance.graphAsset.blackboard;

            if (this.InitializeBlackboard(this._blackboard) == false)
            {
                throw new Exception("Serialized list property is null.");
            }

            this._blackboardBindingField.SetValueWithoutNotify(this._blackboard);
            this.UpdateBlackboardView();
            TaskStreamerEditor.Instance.inspectorView.RefreshInspector();
        }


        /// <summary>컨텍스트 메뉴 창을 열어 블랙보드에 새로운 변수를 생성 및 추가합니다.</summary>
        /// <param name="clickEvent">사용자가 트리거한 클릭 이벤트입니다.</param>
        private void OpenContextualMenuWindow(ClickEvent clickEvent)
        {
            //블랙보드가 null이거나, 현재 그래프를 편집할 수 없는 상태인 경우에는 아무 작업도 하지 않는다.
            if (TaskStreamerEditor.canEditGraph == false || this._blackboard == null)
            {
                return;
            }

            BindingWindow window = BindingWindowBuilder.GetBuilder("Blackboard Variables", false)
                                                       .AddFactoryModule(
                                                           () => new BlackboardVariableFactoryModule("Variables", 0),
                                                           () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<BlackboardVariable>)this.AddVariableToList);
            window.OpenWindow(clickEvent.position);
        }

#endregion

#region Core Update Methods

        /// <summary>블랙보드를 직렬화하고 초기화합니다.</summary>
        /// <param name="blackboard">초기화할 블랙보드 에셋입니다.</param>
        /// <returns>성공적으로 직렬화되었는지 여부를 반환합니다.</returns>
        private bool InitializeBlackboard(BlackboardAsset blackboard)
        {
            if (blackboard == null)
            {
                return false;
            }
            
            this._serializedObject?.Dispose();

            this._serializedObject = new SerializedObject(blackboard);
            this._serializedList = this._serializedObject
                                       .FindProperty("_blackboardData")
                                       ?.FindPropertyRelative("_variables");

            return _serializedObject != null && _serializedList != null;
        }


        /// <summary>블랙보드의 UI 요소를 갱신하고, 아이템 소스와 버전을 업데이트하여 최신 데이터 상태를 반영합니다.</summary>
        private void UpdateBlackboardView()
        {
            if (_blackboard == null)
            {
                return;
            }

            // UI 요소 활성화 설정
            this._variableAddButton.enabledSelf = !Application.isPlaying;
            this._blackboardBindingField.enabledSelf = !Application.isPlaying;

            // 아이템 소스 설정 및 버전 업데이트
            this.itemsSource = this._blackboard.variables;
            this._blackboard.UpdateAppliedVersion();

            // 뷰 새로고침
            this.RefreshItems();
        }

#endregion

#region Event Handlers

        /// <summary>블랙보드 자산 필드에 연결된 자산 변경 이벤트를 처리합니다.</summary>
        /// <param name="changeEvent">새로 바인딩된 블랙보드 자산에 대한 변경 이벤트 정보입니다.</param>
        private void OnBindBlackboardAsset(ChangeEvent<Object> changeEvent)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            if (Undo.isProcessing == false)
            {
                Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer(SetBlackboard)");
            }

            if (this.TryChangeBlackboard(changeEvent.newValue as BlackboardAsset))
            {
                //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
                TaskStreamerEditor.Instance.graphAsset.TrySynchronizeVariablesOfNodes();
                TaskStreamerEditor.Instance.inspectorView.ClearInspector();
                this.ApplyBlackboardChanges();
            }
        }


        /// <summary>블랙보드의 프로퍼티 순서가 변경될 때 호출되는 콜백 메서드입니다.</summary>
        /// <param name="a">변경 전 프로퍼티의 인덱스입니다.</param>
        /// <param name="b">변경 후 프로퍼티의 인덱스입니다.</param>
        private void OnPropertyIndicesSwapped(int a, int b)
        {
            if (TaskStreamerEditor.canEditGraph == false || _blackboard == null || _blackboard.variables is null)
            {
                return;
            }

            if (a == b || a >= _blackboard.count || b >= _blackboard.count)
            {
                return;
            }

            Undo.RecordObject(_blackboard, "Task Streamer (ReorderBlackboardVariable)");
            this.ApplyChangesAndRefresh();
        }


        /// <summary>변수 삭제 요청을 처리하고 목록에서 해당 변수를 제거합니다.</summary>
        /// <param name="variableView">삭제 요청이 발생한 변수의 UI 요소입니다.</param>
        private void OnVariableDeleteRequested(BlackboardVariableView variableView)
        {
            if (base.itemsSource is null || itemsSource.Count == 0)
            {
                return;
            }

            int index = itemsSource.IndexOf(variableView.variable);

            if (index < 0 || index >= itemsSource.Count)
            {
                return;
            }

            this.DeleteVariableFromList(index);
        }


        /// <summary>블랙보드 변수의 키 변경 요청을 처리합니다.</summary>
        /// <param name="variableView">키를 변경할 블랙보드 변수 뷰입니다.</param>
        /// <param name="newName">변경할 새 키 이름입니다.</param>
        private void OnVariableKeyChanged(BlackboardVariableView variableView, string newName)
        {
            if (string.IsNullOrEmpty(newName) || variableView.variable is null)
            {
                return;
            }

            _blackboard.TryRenameKey(variableView.variable, newName);
            this.ApplyChangesAndRefresh();
        }

#endregion

#region Variable Management

        /// <summary>새로운 변수를 블랙보드 리스트에 추가합니다.</summary>
        /// <param name="newBlackboardVariable">블랙보드에 추가할 새 변수입니다.</param>
        private void AddVariableToList(BlackboardVariable newBlackboardVariable)
        {
            Undo.RecordObject(_blackboard, "Task Streamer (AddBlackboardVariable)");
            _blackboard.AddVariable(newBlackboardVariable);
            this.ApplyChangesAndRefresh();
        }


        /// <summary>지정된 인덱스의 블랙보드 변수를 삭제하고 관련 UI와 데이터를 갱신합니다.</summary>
        /// <param name="index">삭제할 블랙보드 변수의 인덱스입니다.</param>
        private void DeleteVariableFromList(int index)
        {
            Object[] objects = { _blackboard, TaskStreamerEditor.Instance.graphAsset };
            Undo.RecordObjects(objects, "Task Streamer (RemoveBlackboardVariable)");

            _blackboard.RemoveVariable(itemsSource[index] as BlackboardVariable);

            //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
            TaskStreamerEditor.Instance.graphAsset.TrySynchronizeVariablesOfNodes();

            this.ApplyChangesAndRefresh();
            TaskStreamerEditor.Instance.inspectorView.RefreshInspector();
        }

#endregion

#region UI Binding

        /// <summary>리스트의 아이템을 UI 요소에 바인딩하여 데이터를 표시하도록 설정합니다.</summary>
        /// <param name="element">바인딩할 대상 UI 요소입니다.</param>
        /// <param name="index">바인딩할 데이터의 인덱스입니다.</param>
        private void BindItemToList(VisualElement element, int index)
        {
            if (_serializedList.arraySize <= index || element is not BlackboardVariableView variableView)
            {
                return;
            }

            SerializedProperty serializedProperty = _serializedList.GetArrayElementAtIndex(index);
            BlackboardVariable blackboardVariable = itemsSource[index] as BlackboardVariable;

            variableView.OnDeleteRequested -= this.OnVariableDeleteRequested;
            variableView.OnKeyChanged -= this.OnVariableKeyChanged;

            variableView.OnDeleteRequested += this.OnVariableDeleteRequested;
            variableView.OnKeyChanged += this.OnVariableKeyChanged;

            variableView.Setup(blackboardVariable, serializedProperty);
        }

#endregion

#region Serialization and Update

        /// <summary>블랙보드의 변경 사항을 적용하고 수정된 정보를 유니티에 반영합니다.</summary>
        private void ApplyBlackboardChanges()
        {
            if (_blackboard != null)
            {
                UnityEditor.EditorUtility.SetDirty(_blackboard);
            }

            if (_serializedObject is null || _serializedObject.targetObject is null)
            {
                return;
            }

            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
        }


        /// <summary>적용된 변경사항을 블랙보드에 반영하고 리스트 뷰를 새로 고칩니다.</summary>
        private void ApplyChangesAndRefresh()
        {
            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }

#endregion
    }
}