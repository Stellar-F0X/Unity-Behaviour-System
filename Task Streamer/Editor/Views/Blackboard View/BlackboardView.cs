using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    [UxmlElement]
    public partial class BlackboardView : ListView
    {
        private SerializedProperty _serializedList;
        private SerializedObject _serializedObject;
        private Blackboard _blackboard;

        private VariableCreationWindow _creationWindow;
        private ObjectField _blackboardBindingField;
        private Button _variableAddButton;


        /// <summary>블랙보드 프로퍼티 리스트 뷰를 초기화하고 UI 요소들을 설정합니다.</summary>
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
        
        
        /// <summary>블랙보드 에셋이 바인딩될 때 호출되는 콜백 메서드입니다.</summary>
        private void OnBindBlackboardAsset(ChangeEvent<Object> changeEvent)
        {
            if (TaskStreamerEditor.canEditGraph == false || changeEvent.newValue is not Blackboard newBlackboard)
            {
                return;
            }
            
            if (this._blackboard != null)
            {
                //1. 블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
                TaskStreamerEditor.Instance.graphAsset.ResetBoundVariables();
            }
            
            //2. 교체.
            this.TrySetupBlackboard(newBlackboard);
            
            TaskStreamerEditor.Instance.graphAsset.blackboard = _blackboard;
        }


        /// <summary>언두 작업이 수행될 때 아이템을 새로고침합니다.</summary>
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


        /// <summary>블랙보드 뷰를 초기화하고 모든 데이터를 제거합니다.</summary>
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


        /// <summary>Graph가 변경될 때 블랙보드 뷰를 업데이트합니다.</summary>
        public void TrySetupBlackboard(Blackboard changeBlackboard)
        {
            if (TaskStreamerEditor.Instance is null)
            {
                return;
            }

            if (changeBlackboard != null && this._blackboard == changeBlackboard)
            {
                return;
            }

            this._blackboard = changeBlackboard;
            this._blackboardBindingField.value = changeBlackboard;
            this._variableAddButton.enabledSelf = !Application.isPlaying;

            if (this._blackboard is null)
            {
                this.ResetItemsOnBlackboardRemoved();
                return;
            }

            this._serializedObject = new SerializedObject(this._blackboard);
            this._serializedList = _serializedObject.FindProperty("_variables");

            if (SerializedProperty.DataEquals(this._serializedList, null))
            {
                Debug.LogWarning("Serialized list property is null.");
                return;
            }

            this.itemsSource = this._blackboard.variables;
            this.RefreshItems();
        }


        private void ResetItemsOnBlackboardRemoved()
        {
            if (base.itemsSource is null)
            {
                return;
            }

            this.itemsSource = null;
            this.RefreshItems();
        }


        /// <summary>블랙보드 프로퍼티를 추가하는 버튼 클릭 이벤트를 처리합니다.</summary>
        public void OpenContextualMenuWindow(ClickEvent clickEvent)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }
            
            if (_creationWindow == null)
            {
                _creationWindow = ScriptableObject.CreateInstance<VariableCreationWindow>();
            }
            
            _creationWindow.RegisterNodeCreationCallbackOnce(this.AddVariableToList);

            Vector2 mousePosition = clickEvent.position;
            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(mousePosition);
            SearchWindowContext context = new SearchWindowContext(screenPoint, 200, 240);

            SearchWindow.Open(context, _creationWindow);
        }


        /// <summary>새로운 블랙보드 프로퍼티를 생성합니다.</summary>
        private void AddVariableToList(Variable newBlackboardVariable)
        {
            Undo.RecordObject(_blackboard, "Task Streamer (AddBlackboardVariable)");

            _blackboard.AddVariable(newBlackboardVariable);

            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }


        /// <summary>지정된 인덱스의 블랙보드 프로퍼티를 삭제합니다.</summary>
        private void DeleteVariableFromList(int index)
        {
            Undo.RecordObject(_blackboard, "Task Streamer (RemoveBlackboardVariable)");

            _blackboard.RemoveVariable(itemsSource[index] as Variable);

            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }


        /// <summary>프로퍼티의 순서가 변경될 때 호출되는 콜백 메서드입니다.</summary>
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

            if (a == b || a >= _blackboard.variables.Count || b >= _blackboard.variables.Count)
            {
                return;
            }

            Undo.RecordObject(_blackboard, "Task Streamer (ReorderBlackboardVariable)");

            this._blackboard.OnAfterDeserialize();

            this.ApplyBlackboardChanges();
            this.RefreshItems();
        }


        /// <summary>리스트 아이템을 UI 요소에 바인딩합니다.</summary>
        private void BindItemToList(VisualElement element, int index)
        {
            if (_serializedList.arraySize <= index)
            {
                return;
            }

            if (element is not BlackboardVariableView variableView)
            {
                return;
            }

            SerializedProperty serializedProperty = _serializedList.GetArrayElementAtIndex(index);
            Variable blackboardVariable = itemsSource[index] as Variable;

            variableView.OnDeleteRequested -= this.OnVariableDeleteRequested;
            variableView.OnNameChanged -= this.OnVariableNameChanged;

            variableView.OnDeleteRequested += this.OnVariableDeleteRequested;
            variableView.OnNameChanged += this.OnVariableNameChanged;

            variableView.Setup(blackboardVariable, serializedProperty);
        }


        /// <summary>변수 삭제 요청을 처리합니다.</summary>
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


        /// <summary>변수 이름 변경 요청을 처리합니다.</summary>
        private void OnVariableNameChanged(BlackboardVariableView variableView, string newName)
        {
            if (string.IsNullOrEmpty(newName) || variableView.variable is null)
            {
                return;
            }

            _blackboard.TryChangeVariableName(variableView.variable, newName);

            this.ApplyBlackboardChanges();
        }


        /// <summary>블랙보드의 변경 사항을 적용합니다.</summary>
        private void ApplyBlackboardChanges()
        {
            if (_serializedObject is null || _serializedObject.targetObject is null)
            {
                return;
            }

            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(_blackboard);
        }
    }
}