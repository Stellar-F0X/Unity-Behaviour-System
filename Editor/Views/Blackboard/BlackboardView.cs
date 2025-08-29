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
        /// Stores the serialized list of variables from a BlackboardAsset, enabling data binding and editor UI updates.
        /// </summary>
        private SerializedProperty _serializedList;

        /// <summary>
        /// Represents a SerializedObject instance used for managing and binding
        /// properties of a BlackboardAsset in the Unity Editor.
        /// </summary>
        private SerializedObject _serializedObject;

        /// <summary>
        /// Represents the underlying data model of type <see cref="BlackboardAsset"/> used by the <see cref="BlackboardView"/> to manage and display variables.
        /// </summary>
        private BlackboardAsset _blackboard;

        /// <summary>
        /// Represents the ObjectField used for selecting and binding a BlackboardAsset in the UI.
        /// </summary>
        private ObjectField _blackboardBindingField;

        /// <summary>
        /// Represents a button used to add variables in the blackboard view.
        /// </summary>
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


        /// <summary>언두 작업이 수행될 때 블랙보드 뷰와 관련된 아이템을 새로고침합니다.</summary>
        public void RefreshItemsWhenUndoPerformed()
        {
            if (_serializedObject?.targetObject is null)
            {
                return;
            }

            this.TrySetupBlackboard(TaskStreamerEditor.Instance.graphAsset.blackboard);
            
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


        /// <summary>블랙보드에서 아이템 소스가 제거되었을 때, 뷰를 초기화하고 새로고침합니다.</summary>
        private void ResetItemsOnBlackboardRemoved()
        {
            if (base.itemsSource is null)
            {
                return;
            }

            this.itemsSource = null;

            this.RefreshItems();

            TaskStreamerEditor.Instance.editorInspectorView.RefreshInspector();
        }


        /// <summary>블랙보드 에셋이 해당 필드에 바인딩되는 이벤트를 처리합니다.</summary>
        /// <param name="changeEvent">바인딩된 블랙보드 에셋 변경 정보가 포함된 이벤트입니다.</param>
        private void OnBindBlackboardAsset(ChangeEvent<Object> changeEvent)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            BlackboardAsset newBlackboard = changeEvent.newValue as BlackboardAsset;

            BlackboardAsset previousBlackboard = this._blackboard;

            if (Undo.isProcessing == false)
            {
                GraphAsset asset = TaskStreamerEditor.Instance.graphAsset;
                
                UndoUtility.RecordObjects("TaskStreamer(SetBlackboard)", asset, _blackboard);
            }

            if (this.TrySetupBlackboard(newBlackboard) == false)
            {
                return;
            }

            //TrySetupBlackboard에서 여러 조건을 비교한 뒤, 새로운 값이 반영된 블랙보드 필드를 에디터 BB에 대입한다. 
            TaskStreamerEditor.Instance.graphAsset.blackboard = newBlackboard;

            if (previousBlackboard != newBlackboard)
            {
                //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
                TaskStreamerEditor.Instance.graphAsset.TrySynchronizeVariablesOfNodes();
                TaskStreamerEditor.Instance.editorInspectorView.ClearInspector();
            }

            if (newBlackboard == null)
            {
                this.ApplyBlackboardChanges();
            }
        }


        /// <summary>새로운 블랙보드 자산으로 블랙보드 뷰를 업데이트합니다.</summary>
        /// <param name="newBlackboard">뷰와 연결할 새로운 블랙보드 자산입니다. null일 경우 뷰를 초기화합니다.</param>
        public bool TrySetupBlackboard(BlackboardAsset newBlackboard)
        {
            //새롭게 들어온 블랙보드가 현재 블랙보드와 동일한 경우에는 아무 작업도 하지 않는다.
            if (this._blackboard == newBlackboard)
            {
                return false;
            }
           
            newBlackboard?.UpdateAppliedVersion();  //업데이트 버전을 갱신한다.
            
            this._blackboard = newBlackboard;
            this._blackboardBindingField.SetValueWithoutNotify(newBlackboard);

            if (newBlackboard is null)
            {
                this.ResetItemsOnBlackboardRemoved();
                return true;
            }

            try
            {
                this._serializedObject = new SerializedObject(newBlackboard);
                
                this._serializedList = this._serializedObject
                                           ?.FindProperty("_blackboardData")
                                           ?.FindPropertyRelative("_variables");
            }
            catch
            {
                Debug.LogError("Serialized list property is null.");
                return false;
            }
            
            this._variableAddButton.enabledSelf = !Application.isPlaying;
            this._blackboardBindingField.enabledSelf = !Application.isPlaying;
            
            //블랙보드가 null이 아닌 경우, 아이템 소스를 블랙보드의 변수 리스트로 설정하고 새로고침한다.
            this.itemsSource = this._blackboard.variables;
            this.RefreshItems();
            return true;
        }


        /// <summary>블랙보드에 새 변수를 추가하기 위해 컨텍스트 메뉴 창을 엽니다.</summary>
        /// <param name="clickEvent">사용자가 트리거한 클릭 이벤트입니다.</param>
        public void OpenContextualMenuWindow(ClickEvent clickEvent)
        {
            //블랙보드가 null이거나, 현재 그래프를 편집할 수 없는 상태인 경우에는 아무 작업도 하지 않는다.
            if (TaskStreamerEditor.canEditGraph == false || this._blackboard == null)
            {
                return;
            }

            Func<FactoryModule> moduleProvider = () => new BlackboardVariableFactoryModule("Variables", 0);

            Func<ICategoryTreeProvider> provider = () => new TypeTreeProvider(true);

            BindingWindow window = BindingWindowBuilder.GetBuilder("Blackboard Variables", false)
                                                       .AddFactoryModule(moduleProvider, provider)
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<BlackboardVariable>)this.AddVariableToList);
            window.OpenWindow(clickEvent.position);
        }


        /// <summary>새로운 변수를 블랙보드 리스트에 추가합니다.</summary>
        /// <param name="newBlackboardVariable">추가될 새 블랙보드 변수입니다.</param>
        private void AddVariableToList(BlackboardVariable newBlackboardVariable)
        {
            Undo.RecordObject(_blackboard, "Task Streamer (AddBlackboardVariable)");

            _blackboard.AddVariable(newBlackboardVariable);

            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }


        /// <summary>블랙보드에서 지정된 인덱스의 프로퍼티를 삭제합니다.</summary>
        /// <param name="index">삭제할 블랙보드 프로퍼티의 인덱스입니다.</param>
        private void DeleteVariableFromList(int index)
        {
            UndoUtility.RecordObjects("Task Streamer (RemoveBlackboardVariable)", _blackboard, TaskStreamerEditor.Instance.graphAsset);

            _blackboard.RemoveVariable(itemsSource[index] as BlackboardVariable);

            //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
            TaskStreamerEditor.Instance.graphAsset.TrySynchronizeVariablesOfNodes();

            this.ApplyBlackboardChanges();
            this.RefreshItems();

            TaskStreamerEditor.Instance.editorInspectorView.RefreshInspector();
        }


        /// <summary>블랙보드의 프로퍼티 순서가 변경될 때 호출되는 콜백 메서드입니다.</summary>
        /// <param name="a">변경 전 프로퍼티의 인덱스입니다.</param>
        /// <param name="b">변경 후 프로퍼티의 인덱스입니다.</param>
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


        /// <summary>변수에 대한 삭제 요청을 처리하고 해당 변수를 리스트에서 제거합니다.</summary>
        /// <param name="variableView">삭제 요청이 발생한 변수를 나타내는 UI 요소입니다.</param>
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
        /// <param name="variableView">키가 변경될 변수와 연관된 뷰입니다.</param>
        /// <param name="newName">변수의 새 키 이름입니다.</param>
        private void OnVariableKeyChanged(BlackboardVariableView variableView, string newName)
        {
            if (string.IsNullOrEmpty(newName) || variableView.variable is null)
            {
                return;
            }

            _blackboard.TryRenameKey(variableView.variable, newName);

            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }


        /// <summary>블랙보드의 변경 사항을 적용하여 직렬화된 객체를 업데이트하고 수정된 속성을 적용하며 블랙보드를 변경 상태로 표시합니다.</summary>
        private void ApplyBlackboardChanges()
        {
            if (_serializedObject is null || _serializedObject.targetObject is null)
            {
                return;
            }

            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();

            if (_blackboard != null)
            {
                UnityEditor.EditorUtility.SetDirty(_blackboard);
            }
        }
    }
}