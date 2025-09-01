using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer;
using TaskStreamer.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary> DefaultPanel 클래스는 작업(Task) 또는 전환(Transition)을 표시하고, 수정할 수 있는 기본 패널 UI를 제공합니다. </summary>
    public class BasicSectionPanel : VisualElement, IRefreshablePanel
    {
        /// <summary> 작업(Task)의 기본 속성을 표시하고 편집할 수 있는 UI 패널입니다. </summary>
        public BasicSectionPanel(Task targetTask, Action<string> renamingCallback)
        {
            TaskStreamerResourcesLoader.BasicSectionPanel.CloneTree(this);

            _nameField = this.Q<TextField>("name-field");
            _monoScriptField = this.Q<ObjectField>("script-field");
            _tagSelectionField = this.Q<DropdownField>("tag-field");
            _desContentField = this.Q<TextField>("description-content");
            _baseTitleHeader = this.Q<Label>("base-title-header");

            _targetTask = targetTask;
            _nodeType = targetTask.GetType();
            _monoScriptField.value = EditorUtility.GetMonoScriptFromPoco(_nodeType);
            _renamingCallback = renamingCallback;

            this.InitializeFields(targetTask);

            _nameField.RegisterValueChangedCallback(evt => _renamingCallback?.Invoke(evt.newValue));
            _tagSelectionField.RegisterValueChangedCallback(evt => targetTask.tag = evt.newValue);
            _desContentField.RegisterValueChangedCallback(evt => targetTask.description = evt.newValue);
        }

        
        
        /// <summary>작업의 태그를 선택하는 DropdownField UI 요소입니다.</summary>
        private readonly DropdownField _tagSelectionField;


        /// <summary>작업 이름이 변경될 때 호출되는 사용자 정의 콜백 이벤트를 저장합니다.</summary>
        private event Action<string> _renamingCallback;


        /// <summary>현재 노드의 클래스 타입 정보를 저장하는 변수입니다.</summary>
        private readonly Type _nodeType;


        /// <summary>패널에서 처리 및 표시할 대상 작업(Task)을 나타냅니다.</summary>
        private readonly Task _targetTask;


        /// <summary>작업(Task)의 기본 제목을 표시하는 UI 라벨 요소입니다.</summary>
        private readonly Label _baseTitleHeader;


        /// <summary>작업(Task)의 이름을 입력하거나 수정할 수 있는 TextField입니다.</summary>
        private readonly TextField _nameField;


        /// <summary>Task의 설명 내용을 입력하거나 수정하는 데 사용되는 텍스트 필드입니다.</summary>
        private readonly TextField _desContentField;

        
        /// <summary>Task의 관련 MonoScript를 표시하고 설정하기 위해 사용하는 ObjectField UI 요소입니다.</summary>
        private readonly ObjectField _monoScriptField;



        /// <summary> 패널 데이터를 갱신하여 최신 상태를 반영합니다. </summary>
        public void RefreshPanel()
        {
            this.InitializeFields(this._targetTask);
        }



        /// <summary> Task의 필드를 초기화하고, UI 요소를 갱신합니다. </summary>
        /// <param name="task"> 초기화 대상이 되는 Task 객체입니다. </param>
        private void InitializeFields(Task task)
        {
            _nameField.enabledSelf = task.canEditName;
            _nameField.SetValueWithoutNotify(task.name);
            _desContentField.SetValueWithoutNotify(task.description);
            _baseTitleHeader.text = ObjectNames.NicifyVariableName(_nodeType.Name);

            List<string> tags = TaskStreamerEditor.settings.tagList;
            _tagSelectionField.value = tags.IndexOf(task.tag) == -1 ? tags[0] : task.tag;
            _tagSelectionField.choices = tags.Where(tag => tag.IsNotNullOrEmpty()).ToList();
        }
    }
}