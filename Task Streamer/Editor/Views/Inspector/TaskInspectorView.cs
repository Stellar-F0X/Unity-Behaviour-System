using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Injection;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> TaskInspectorView 클래스는 작업(Task)의 속성을 표시하고 편집을 위한 VisualElement를 정의합니다. </summary>
    public class TaskInspectorView : VisualElement
    {
        public TaskInspectorView(Task targetTask, Action<string> renamingCallback, IReadOnlyList<object> fieldProperties)
        {
            TaskStreamerEditor.settings.nodeInspectorXml.CloneTree(this); 

            VisualElement baseContainer = this.Q<VisualElement>("base");
            VisualElement childContainer = this.Q<VisualElement>("child");

            _nameField = baseContainer.Q<TextField>("name-field"); 
            _monoScriptField = baseContainer.Q<ObjectField>("script-field"); 
            _tagSelectionField = baseContainer.Q<DropdownField>("tag-field"); 
            _desContentField = baseContainer.Q<TextField>("description-content");
            _baseTitleHeader = baseContainer.Q<Label>("base-title-header");
            
            _childTitleHeader = childContainer.Q<Label>("child-title-header"); 
            _fieldContainer = childContainer.Q<VisualElement>("property-container"); 
            
            this._fieldProperties = fieldProperties;
            this._renamingCallback = renamingCallback;

            this.SetupDefaultFields(targetTask);
            this.SetupBlackboardVariableFields(fieldProperties);
            this.ControlTitleHeader();
        }

        
        /// <summary>작업 이름 변경에 대한 콜백을 처리하는 이벤트입니다.</summary>
        private event Action<string> _renamingCallback;

        
        /// <summary> Represents the type information of the current node. </summary>
        private Type _nodeType;

        
        /// <summary>대상 작업(Task)을 나타내는 변수</summary>
        private Task _targetTask;


        /// <summary> Task의 필드 데이터를 담고 있는 속성 리스트를 나타냅니다. </summary>
        private IReadOnlyList<object> _fieldProperties;
        
        
        
        private readonly Label _baseTitleHeader;

        
        /// <summary>TextField UI element for displaying and editing the task's name.</summary>
        private readonly TextField _nameField;

        
        /// <summary> MonoScript를 작업 노드와 연결하는 ObjectField. </summary>
        private readonly ObjectField _monoScriptField;

        
        /// <summary> Dropdown field used for tag selection. </summary>
        private readonly DropdownField _tagSelectionField;


        
        /// <summary> Task의 설명 내용을 편집하는 데 사용되는 필드입니다. </summary>
        private readonly TextField _desContentField;

        
        /// <summary> Represents a header label element for child-related titles in the TaskInspectorView. </summary>
        private readonly Label _childTitleHeader;

        
        /// <summary> 태스크의 속성 값을 표시하는 컨테이너 역할을 수행합니다. </summary>
        private readonly VisualElement _fieldContainer;

        



        /// <summary> Task의 기본 필드를 초기화한다. </summary>
        /// <param name="task"> 초기화할 대상 Task 객체 </param>
        private void SetupDefaultFields(Task task)
        {
            this._targetTask = task;
            this._nodeType = task.GetType();
            
            this.InitializeTaskFields(task);
            
            this._monoScriptField.value = EditorUtility.GetMonoScriptFromPoco(_nodeType);

            List<string> tags = TaskStreamerEditor.settings.tagList;

            if (tags is not null && tags.Count > 0)
            {
                _tagSelectionField.value = tags.IndexOf(task.tag) == -1 ? tags[0] : task.tag;
                _tagSelectionField.choices = tags.Where(tag => tag.IsNotNullOrEmpty()).ToList();
            }

            _nameField.RegisterValueChangedCallback(evt => _renamingCallback?.Invoke((_targetTask.name = evt.newValue)));
            _desContentField.RegisterValueChangedCallback(evt => task.description = evt.newValue);
            _tagSelectionField.RegisterValueChangedCallback(evt => task.tag = evt.newValue);
        }

        

        /// <summary> Task의 필드 값을 초기화한다. </summary>
        /// <param name="task"> 초기화할 대상 Task 객체입니다. </param>
        private void InitializeTaskFields(Task task)
        {
            _nameField.enabledSelf = task.canEditName;
            _nameField.SetValueWithoutNotify(task.name); 
            _desContentField.SetValueWithoutNotify(task.description); 
            _baseTitleHeader.text = ObjectNames.NicifyVariableName(_nodeType.Name); 
        }


        
        /// <summary> 주어진 필드 속성을 기반으로 블랙보드 변수 필드를 설정합니다. </summary>
        /// <param name="fieldProperties"> 블랙보드 변수 필드를 생성하기 위한 필드 속성 목록입니다. </param>
        private void SetupBlackboardVariableFields(IReadOnlyList<object> fieldProperties)
        {
            if (fieldProperties.Count == 0)
            {
                return;
            }

            foreach (VariableHandle property in fieldProperties)
            {
                switch (property.value)
                {
                    case BlackboardVariable: _fieldContainer.Add(VisualUtility.GetFieldByValueType(property)); break;

                    case BlackboardBasedCondition: _fieldContainer.Add(new BlackboaradBasedConditionListField(property)); break;
                }
            }

        }

        

        /// <summary> 필드를 초기화하고 태스크 인스펙터 뷰를 새로고침합니다. </summary>
        public void RefreshAllFields()
        {
            //필드가 없다면 업데이트를 할 필요가 없으므로, 함수를 종료한다.
            if (this._fieldProperties is null || _fieldContainer.childCount == 0)
            {
                return;
            }
            
            this._fieldContainer.Clear();
            this.InitializeTaskFields(this._targetTask);
            this.SetupBlackboardVariableFields(this._fieldProperties);
            this.ControlTitleHeader();
        }



        private void ControlTitleHeader()
        {
            if (_fieldProperties.Count == 0 || _fieldContainer.childCount == 0)
            {
                _childTitleHeader.style.display = DisplayStyle.None;
            }
            else
            {
                _childTitleHeader.style.display = DisplayStyle.Flex;
            }
        }
    }
}