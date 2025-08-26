using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// Represents a custom visual element that serves as a ListView for managing Blackboard properties in a UI.
    /// This class provides functionality to initialize and configure UI elements for the Blackboard,
    /// handle contextual menu events, and update the Blackboard view based on user interactions or asset changes.
    /// </summary>
    [UxmlElement]
    public partial class BlackboardView : ListView
    {
        /// <summary>
        /// Represents the serialized property that holds a list of variables from a BlackboardAsset instance,
        /// allowing for interaction and management within the UnityEditor UI.
        /// </summary>
        private SerializedProperty _serializedList;

        /// <summary>
        /// Represents a SerializedObject instance used for binding the BlackboardAsset
        /// and managing its properties within the blackboard view in the Unity Editor.
        /// </summary>
        /// <remarks>
        /// The SerializedObject encapsulates the serialized representation of the
        /// <see cref="BlackboardAsset"/> instance, enabling property binding, updates,
        /// and manipulation of the asset's data in the Unity UI. This is particularly
        /// useful for managing the variables within the blackboard and reflects changes
        /// in the Unity Editor.
        /// </remarks>
        private SerializedObject _serializedObject;

        /// <summary>
        /// Represents the associated <see cref="BlackboardAsset"/> used within the <see cref="BlackboardView"/> class.
        /// This variable serves as the underlying data model holding the list of variables managed and displayed
        /// by the current instance of the blackboard.
        /// </summary>
        /// <remarks>
        /// The _blackboard variable stores a reference to the active <see cref="BlackboardAsset"/> object.
        /// When a new <see cref="BlackboardAsset"/> is assigned, associated UI components and bindings are updated accordingly.
        /// It acts as a data source for managing and manipulating tasks or variables within the editor environment.
        /// </remarks>
        private BlackboardAsset _blackboard;

        /// <summary>
        /// Represents an ObjectField used for binding a Blackboard asset in the user interface.
        /// </summary>
        /// <remarks>
        /// This field allows the user to select and bind a <see cref="BlackboardAsset"/> to the view.
        /// It is set up during the initialization process and is linked to various events, such as
        /// value changes, to handle binding operations and updates to the associated Blackboard.
        /// </remarks>
        private ObjectField _blackboardBindingField;

        /// <summary>
        /// Represents a button used to add variables to the blackboard in the blackboard view.
        /// </summary>
        /// <remarks>
        /// This button enables functionality to open a contextual menu window for adding new variables.
        /// It is registered and unregistered with an event handler for click events and its state is adjusted
        /// based on the application's runtime context or when the blackboard is cleared or reassigned.
        /// </remarks>
        private Button _variableAddButton;


        /// <summary>블랙보드 프로퍼티 리스트 뷰를 초기화하고 UI 요소들을 설정합니다.</summary>
        /// <param name="variableAddButton">블랙보드에 변수를 추가하기 위한 버튼입니다.</param>
        /// <param name="blackboardBindingField">블랙보드와의 바인딩을 설정하기 위한 오브젝트 필드입니다.</param>
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


        /// <summary>Handles the event when the blackboard asset is bound to the corresponding field.</summary>
        /// <param name="changeEvent">The event containing information about the change in the bound blackboard asset.</param>
        private void OnBindBlackboardAsset(ChangeEvent<Object> changeEvent)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            BlackboardAsset newBlackboard = changeEvent.newValue as BlackboardAsset;

            if (newBlackboard == null && this._blackboard != null)
            {
                //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
                TaskStreamerEditor.Instance.graphAsset.TryCleanUpBoundVariables();
            }

            //2. 교체.
            this.TrySetupBlackboard(newBlackboard);

            TaskStreamerEditor.Instance.graphAsset.blackboard = _blackboard;
            TaskStreamerEditor.Instance.inspectorView.ClearInspectorView();
        }


        /// <summary>
        /// 언두 작업이 수행될 때 블랙보드 뷰와 관련된 아이템을 새로고침합니다.
        /// </summary>
        public void RefreshItemsWhenUndoPerformed()
        {
            if (_serializedObject?.targetObject is null)
            {
                return;
            }

            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();

            this.RefreshItems();
        }


        /// <summary>초기화를 수행하여 블랙보드 뷰의 모든 정보를 제거하고 초기 상태로 되돌립니다.</summary>
        public void ClearBlackboardView()
        {
            this.itemsSource = null;
            this._blackboard = null;
            this._serializedList = null;
            this._serializedObject = null;
            this._variableAddButton.clickable = null;
            this._blackboardBindingField.SetValueWithoutNotify(null);

            this.Clear();
            this.RefreshItems();
        }


        /// <summary>
        /// Updates the Blackboard view with a new Blackboard asset.
        /// </summary>
        /// <param name="newBlackboard">The new Blackboard asset to associate with the view. If null, the view will be reset.</param>
        public void TrySetupBlackboard(BlackboardAsset newBlackboard)
        {
            //새롭게 들어온 블랙보드가 null이거나, 현재 블랙보드와 동일한 경우에는 아무 작업도 하지 않는다.
            if (newBlackboard != null && this._blackboard == newBlackboard)
            {
                return;
            }

            newBlackboard?.UpdateAppliedVersion();

            this._blackboard = newBlackboard;
            this._blackboardBindingField.value = newBlackboard;
            this._variableAddButton.enabledSelf = !Application.isPlaying;
            this._blackboardBindingField.enabledSelf = !Application.isPlaying;

            if (newBlackboard is null)
            {
                //블랙보드가 null인 경우, 아이템 소스(Variable 배열)를 초기화하고 새로고침한다.
                this.ResetItemsOnBlackboardRemoved();
                return;
            }

            this._serializedObject = new SerializedObject(this._blackboard);
            this._serializedList = _serializedObject.FindProperty("_variables");

            if (SerializedProperty.DataEquals(this._serializedList, null))
            {
                //블랙보드의 변수 리스트가 null인 경우, 경고 메시지를 출력하고 초기화한다.
                //이 경우 대부분의 경우는 필드 변수의 이름이 수정된 경우.
                Debug.LogWarning("Serialized list property is null.");
                return;
            }

            //블랙보드가 null이 아닌 경우, 아이템 소스를 블랙보드의 변수 리스트로 설정하고 새로고침한다.
            this.itemsSource = this._blackboard.variables;
            this.RefreshItems();
        }


        /// <summary>
        /// Resets the blackboard view by clearing the current items source and refreshing the view.
        /// </summary>
        private void ResetItemsOnBlackboardRemoved()
        {
            if (base.itemsSource is null)
            {
                return;
            }

            this.itemsSource = null;
            this.RefreshItems();
        }


        /// <summary>
        /// Handles the click event for adding a new variable to the blackboard by opening a contextual menu window.
        /// </summary>
        /// <param name="clickEvent">The click event triggered by the user.</param>
        public void OpenContextualMenuWindow(ClickEvent clickEvent)
        {
            //블랙보드가 null이거나, 현재 그래프를 편집할 수 없는 상태인 경우에는 아무 작업도 하지 않는다.
            if (TaskStreamerEditor.canEditGraph == false || this._blackboard == null)
            {
                return;
            }

            BindingWindow window = BindingWindowBuilder.GetBuilder("Blackboard Variables", false)
                                                       .AddFactoryModule(() => new BBVariableFactoryModule("Variables", 0), () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<BlackboardVariable>)this.AddVariableToList);
            window.OpenWindow(clickEvent.position);
        }


        /// <summary>새로운 변수를 블랙보드 리스트에 추가합니다.</summary>
        /// <param name="newBlackboardVariable">추가될 새 블랙보드 변수.</param>
        private void AddVariableToList(BlackboardVariable newBlackboardVariable)
        {
            Undo.RecordObject(_blackboard, "Task Streamer (AddBlackboardVariable)");

            _blackboard.AddVariable(newBlackboardVariable);

            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }


        /// <summary>Deletes the blackboard property at the specified index.</summary>
        /// <param name="index">The index of the blackboard property to delete.</param>
        private void DeleteVariableFromList(int index)
        {
            Undo.RecordObject(_blackboard, "Task Streamer (RemoveBlackboardVariable)");

            _blackboard.RemoveVariable(itemsSource[index] as BlackboardVariable);

            //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
            TaskStreamerEditor.Instance.graphAsset.TryCleanUpBoundVariables();

            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }


        /// <summary>
        /// Callback method invoked when the order of properties in the blackboard is changed.
        /// </summary>
        /// <param name="a">The original index of the property.</param>
        /// <param name="b">The new index of the property after reordering.</param>
        private void OnPropertyIndicesSwapped(int a, int b)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            if (_blackboard == null || _blackboard.variables is null)
            {
                return;
            }

            if (a == b || a >= _blackboard.count || b >= _blackboard.count)
            {
                return;
            }

            Undo.RecordObject(_blackboard, "Task Streamer (ReorderBlackboardVariable)");

            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }


        /// <summary>리스트 아이템을 UI 요소에 바인딩합니다.</summary>
        /// <param name="element">바인딩할 UI 요소를 나타내는 VisualElement입니다.</param>
        /// <param name="index">바인딩할 아이템의 인덱스입니다.</param>
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


        /// <summary>
        /// Handles the variable delete request by identifying the associated variable and removing it from the list.
        /// </summary>
        /// <param name="variableView">The UI element representing the variable for which the delete request is triggered.</param>
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


        /// <summary>
        /// Handles the request to change the key of a variable in the blackboard.
        /// </summary>
        /// <param name="variableView">The view associated with the variable whose key is being changed.</param>
        /// <param name="newName">The new key name for the variable.</param>
        private void OnVariableKeyChanged(BlackboardVariableView variableView, string newName)
        {
            if (string.IsNullOrEmpty(newName) || variableView.variable is null)
            {
                return;
            }

            _blackboard.TryRenameKey(variableView.variable, newName);

            this.ApplyBlackboardChanges();
        }


        /// <summary>
        /// Applies the changes made to the blackboard by updating the serialized object,
        /// applying the modified properties, and marking the blackboard as dirty.
        /// </summary>
        private void ApplyBlackboardChanges()
        {
            if (_serializedObject is null || _serializedObject.targetObject is null)
            {
                return;
            }

            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();

            UnityEditor.EditorUtility.SetDirty(_blackboard);
        }
    }
}