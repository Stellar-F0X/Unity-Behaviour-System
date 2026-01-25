using System;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// BlackboardView 클래스는 Unity에서 그래프 기반 인터페이스의 블랙보드 관리 기능을 제공하며,
    /// 변수 관리, UI 갱신, 블랙보드 객체 변경 등을 처리합니다.
    /// </summary>
    internal class FloatingBlackboardView : BlackboardViewBase
    {
        public FloatingBlackboardView(TaskGraphView taskGraphView)
        {
            base.title = "Blackboard";
            this.style.left = 15;
            this.style.top = 15;
            this.graphView = taskGraphView;
            
            this.addItemRequested += this.OpenContextualMenuWindow;
            this.editTextRequested += this.OnEditElementTitleText;
            this.removeItemRequest += this.OnRemoveItemRequest;
            
            this.styleSheets.Add(TSEditor.blackboardStyle);
            
            this._addElementButton = this.Q<Button>("addButton");
            this._contentContainer = this.Q<VisualElement>("contentContainer");
        }


        /// `_contentContainer`는 블랙보드 항목을 담는 컨테이너 역할을 하는 VisualElement입니다.
        /// 블랙보드 항목 초기화 및 UI 필드 갱신에 사용됩니다.
        private readonly VisualElement _contentContainer;


        /// <summary>
        /// "_addElementButton"는 BlackboardView 내에서 새 요소를 추가하기 위한 UI 버튼입니다.
        /// 주로 사용자 입력에 따라 요소를 생성하거나 관련 로직을 호출하는 데 사용됩니다.
        /// </summary>
        private readonly Button _addElementButton;



        /// <summary>
        /// 현재 그래프의 블랙보드 데이터를 가져오거나 설정하는 속성입니다.
        /// TaskStreamerEditor의 blackboard에 접근하거나 값을 업데이트할 수 있습니다.
        /// </summary>
        private BlackboardAsset blackboard
        {
            get
            {
                if (TSEditor.hasBlackboard)
                {
                    return TSEditor.Instance.graphAsset?.blackboard;
                }

                return null;
            }
            
            set
            {
                if (TSEditor.Instance.graphAsset == null)
                {
                    return;
                }

                TSEditor.Instance.graphAsset.blackboard = value;
            }
        }
        
        


        /// <summary>변수 리스트를 다시 렌더링하여 블랙보드 뷰를 갱신합니다.</summary>
        public void OnUndoPerformed()
        {
            this._contentContainer.Clear();

            if (TSEditor.Instance.graphAsset == null)
            {
                return;
            }

            this.blackboard = TSEditor.Instance.graphAsset.blackboard;

            if (blackboard == null)
            {
                return;
            }
            
            foreach (BlackboardVariable variable in blackboard.variables)
            {
                _contentContainer.Add(this.CreateBlackboardField(variable));
            }
        }



        /// <summary>블랙보드 뷰를 초기화하여 UI 요소를 정리하고 새롭게 설정합니다.</summary>
        public void ClearView()
        {
            this.Clear();
            TSEditor.Instance.inspectorView.ClearInspector();
        }



        public void Show(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                this.style.display = DisplayStyle.Flex;
            }
            else
            {
                this.style.display = DisplayStyle.None;
            }
        }

        

        /// <summary>새로운 블랙보드 자산을 설정하고 뷰를 업데이트합니다.</summary>
        /// <param name="newBlackboard">설정할 새로운 블랙보드 자산입니다.</param>
        /// <returns>블랙보드 변경 여부를 나타내는 값입니다.</returns>
        public void ChangeBlackboard(BlackboardAsset newBlackboard)
        {
            this.blackboard?.UpdateAppliedVersion();
            this.blackboard = newBlackboard;
            
            if (newBlackboard != null)
            {
                this.UpdateView(newBlackboard);
            }
            else
            {
                this.ClearView();
            }
        }


        
        /// <summary>블랙보드 데이터를 기반으로 UI를 새롭게 갱신합니다.</summary>
        /// <param name="newBlackboard">갱신에 사용할 새로운 블랙보드 데이터를 포함한 객체입니다.</param>
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

            // UI 요소 활성화 설정
            this._addElementButton.enabledSelf = !Application.isPlaying;
        }



        /// <summary>블랙보드 변수에 대한 BlackboardField를 생성합니다.</summary>
        /// <param name="variable">생성에 사용될 블랙보드 변수 객체입니다.</param>
        /// <returns>생성된 BlackboardField 객체를 반환합니다.</returns>
        private BlackboardField CreateBlackboardField(BlackboardVariable variable)
        {
            BlackboardField fieldView = new BlackboardField
            {
                typeText = StringUtility.ToNicifyName(variable.valueType.Name),
                userData = variable,
                text = variable.key,
            };

            return fieldView;
        }



        /// <summary>블랙보드에 변수를 기록하고 추가합니다.</summary>
        /// <param name="variable">추가할 블랙보드 변수입니다.</param>
        private void RecordAndAddVariable(BlackboardVariable variable)
        {
            Undo.RecordObject(blackboard, "Task Streamer (AddBlackboardVariable)");
            this.blackboard.AddVariable(variable);
            this.Add(this.CreateBlackboardField(variable));
        }



#region Blackboard Interact Events
        /// <summary>컨텍스트 메뉴 창을 열어 블랙보드에 새로운 변수를 추가합니다.</summary>
        private void OpenContextualMenuWindow(Blackboard blackboardView)
        {
            //블랙보드가 null이거나, 현재 그래프를 편집할 수 없는 상태인 경우에는 아무 작업도 하지 않는다.
            if (TSEditor.canEditGraph == false || this.blackboard == null)
            {
                return;
            }

            BindingWindow window = BindingWindowBuilder.GetBuilder("Blackboard Variables", false)
                                                       .AddFactoryModule(
                                                           () => new BBVariableFactoryModule("Variables", 0),
                                                           () => new BBVariableTypeProvider())
                                                       .Build();

            window.RegisterCreationCallbackOnce((Action<BlackboardVariable>)RecordAndAddVariable);
            window.OpenWindow(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
        }



        /// <summary>블랙보드 요소의 제목 텍스트를 편집합니다.</summary>
        /// <param name="blackboardView">현재 블랙보드 뷰 인스턴스입니다.</param>
        /// <param name="blackboardField">수정 대상이 되는 블랙보드 필드입니다.</param>
        /// <param name="newName">새롭게 지정할 제목 텍스트입니다.</param>
        private void OnEditElementTitleText(Blackboard blackboardView, VisualElement blackboardField, string newName)
        {
            if (blackboardField is not BlackboardField field)
            {
                Debug.LogError("Field is not a valid BlackboardField type");
                return;
            }
            
            if (blackboardField.userData is not BlackboardVariable variable)
            {
                Debug.LogError("userData is not a valid BlackboardVariable type");
                return;
            }

            if (blackboard.TryRenameKey(variable, newName))
            {
                field.text = newName;
                UnityEditor.EditorUtility.SetDirty(this.blackboard);
            }
        }

        

        /// <summary>블랙보드에서 선택된 아이템을 제거하고 관련 데이터를 갱신합니다.</summary>
        /// <param name="blackboardView">아이템이 제거될 블랙보드 뷰입니다.</param>
        /// <param name="blackboardField">제거할 블랙보드 필드 아이템입니다.</param>
        private void OnRemoveItemRequest(Blackboard blackboardView, BlackboardField blackboardField)
        {
            BlackboardVariable foundVariable = blackboardField.userData as BlackboardVariable;
            Debug.Assert(foundVariable is not null, "foundVariable is null");
            
            Object[] objects = { blackboard, TSEditor.Instance.graphAsset };
            Undo.RecordObjects(objects, "Task Streamer (RemoveBlackboardVariable)");

            blackboard.RemoveVariable(foundVariable);

            //블랙보드가 교체될 때, 기존 블랙보드가 있었다면 블랙보드의 변수가 등록되어 있는 노드들의 variable들을 초기화.
            TSEditor.Instance.graphAsset.TrySynchronizeVariablesOfNodes();
            TSEditor.Instance.inspectorView.RefreshInspector();
            
            UnityEditor.EditorUtility.SetDirty(blackboard);
            UnityEditor.EditorUtility.SetDirty(TSEditor.Instance.graphAsset);
        }
#endregion
    }
}