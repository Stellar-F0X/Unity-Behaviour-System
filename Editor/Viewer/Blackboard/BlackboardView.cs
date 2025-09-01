using System;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    public class BlackboardView : BlackboardBase
    {
        public BlackboardView()
        {
            this.styleSheets.Add(TaskStreamerResourcesLoader.BlackboardStyle);
            
            this.title = "Blackboard";
            this.subTitle = string.Empty;
            
            this._blackboardField = new ObjectField();
            this._blackboardField.objectType = typeof(BlackboardAsset);
            this._blackboardField.RegisterValueChangedCallback(this.OnChangeBlackboard);
            this.Add(_blackboardField);
            
            this._addElementButton = this.Q<Button>("addButton");
            
            this.addItemRequested += this.OpenContextualMenuWindow;
            this.editTextRequested += this.OnEditElementTitleText;
            this.moveItemRequested += this.OnMoveItemRequested;
            this.removeItemRequest += this.OnRemoveItemRequest;
        }


        private ObjectField _blackboardField;

        private Button _addElementButton;


        private BlackboardAsset blackboard
        {
            get
            {
                if (TaskStreamerEditor.hasBlackboard)
                {
                    return TaskStreamerEditor.Instance.graphAsset?.blackboard;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (TaskStreamerEditor.Instance.graphAsset is null)
                {
                    return;
                }

                TaskStreamerEditor.Instance.graphAsset.blackboard = value;
                this._blackboardField.SetValueWithoutNotify(value);
            }
        }
        


        private void RecordAndAddVariable(BlackboardVariable variable)
        {
            Undo.RecordObject(blackboard, "Task Streamer (AddBlackboardVariable)");
            this.blackboard.AddVariable(variable);
            this.Add(this.CreateBlackboardField(variable));
        }


        public void OnUndoPerformed() { }


        private BlackboardField CreateBlackboardField(BlackboardVariable variable)
        {
            BlackboardField fieldView = new BlackboardField();
            string typeName = variable.implementedType.Name.Replace("Variable", "");

            fieldView.typeText = StringUtility.ToNicifyName(typeName);
            fieldView.text = variable.key;
            return fieldView;
        }


        public bool TryChangeBlackboard(BlackboardAsset newBlackboard)
        {
            this.blackboard = newBlackboard;

            if (newBlackboard is null)
            {
                this.ClearView();
            }
            else
            {
                this.UpdateView(newBlackboard);
            }

            return true;
        }


        public void ClearView()
        {
            this.Clear();
            this.Add(_blackboardField);
            this._blackboardField.SetValueWithoutNotify(null);
            TaskStreamerEditor.Instance.inspectorView.ClearInspector();
        }


        private void OnChangeBlackboard(ChangeEvent<Object> evt)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            if (Undo.isProcessing == false)
            {
                Undo.RecordObject(TaskStreamerEditor.Instance.graphAsset, "TaskStreamer(SetBlackboard)");
            }

            if (this.TryChangeBlackboard(evt.newValue as BlackboardAsset))
            {
                //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
                TaskStreamerEditor.Instance.graphAsset.TrySynchronizeVariablesOfNodes();
                TaskStreamerEditor.Instance.inspectorView.ClearInspector();
            }
        }


        private void UpdateView(BlackboardAsset newBlackboard)
        {
            if (blackboard == null)
            {
                return;
            }

            this.ClearView();

            foreach (BlackboardVariable variable in newBlackboard.variables)
            {
                this.Add(CreateBlackboardField(variable));
            }

            this.blackboard = newBlackboard;

            // UI 요소 활성화 설정
            this._addElementButton.enabledSelf = !Application.isPlaying;
            this._blackboardField.enabledSelf = !Application.isPlaying;
        }


        /// <summary>컨텍스트 메뉴 창을 열어 블랙보드에 새로운 변수를 생성 및 추가합니다.</summary>
        private void OpenContextualMenuWindow(Blackboard blackboardView)
        {
            //블랙보드가 null이거나, 현재 그래프를 편집할 수 없는 상태인 경우에는 아무 작업도 하지 않는다.
            if (TaskStreamerEditor.canEditGraph == false || this.blackboard == null)
            {
                return;
            }

            BindingWindow window = BindingWindowBuilder.GetBuilder("Blackboard Variables", false)
                                                       .AddFactoryModule(
                                                           () => new BlackboardVariableFactoryModule("Variables", 0),
                                                           () => new TypeTreeProvider(true))
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<BlackboardVariable>)RecordAndAddVariable);
            window.OpenWindow(EditorGUIUtility.GUIToScreenPoint(Event.current.mousePosition));
        }



        private void OnEditElementTitleText(Blackboard arg1, VisualElement arg2, string arg3) { }



        private void OnMoveItemRequested(Blackboard arg1, int arg2, VisualElement arg3) { }



        private void OnRemoveItemRequest(Blackboard blackboard1, BlackboardField blackboardField) { }
    }
}