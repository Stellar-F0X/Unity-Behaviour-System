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
            _childTitleHeader = childContainer.Q<Label>("child-title-header");
            _fieldContainer = childContainer.Q<VisualElement>("property-container");

            this._targetTask = targetTask;
            this._nodeType = targetTask.GetType();
            this._renamingCallback = renamingCallback;

            this.InitializeFields();
            this.RegisterFields(fieldProperties);
        }


        private Type _nodeType;
        private Task _targetTask;
        private Action<string> _renamingCallback;

        private readonly TextField _nameField;
        private readonly ObjectField _monoScriptField;
        private readonly DropdownField _tagSelectionField;
        private readonly TextField _desContentField;
        private readonly Label _childTitleHeader;
        private readonly VisualElement _fieldContainer;


        private void InitializeFields()
        {
            _nameField.value = _targetTask.name;
            _nameField.enabledSelf = _targetTask.canEditName;
            _desContentField.value = _targetTask.description;
            _childTitleHeader.text = ObjectNames.NicifyVariableName(_nodeType.Name);
            _monoScriptField.value = EditorUtility.GetMonoScriptFromPoco(_nodeType);

            List<string> tags = TaskStreamerEditor.settings.tagList;

            if (tags is not null && tags.Count > 0)
            {
                _tagSelectionField.value = tags.IndexOf(_targetTask.tag) == -1 ? tags[0] : _targetTask.tag;
                _tagSelectionField.choices = tags.Where(tag => tag.IsNotNullOrEmpty()).ToList();
            }

            _nameField.RegisterValueChangedCallback(evt => _renamingCallback?.Invoke((_targetTask.name = evt.newValue)));
            _desContentField.RegisterValueChangedCallback(evt => _targetTask.description = evt.newValue);
            _tagSelectionField.RegisterValueChangedCallback(evt => _targetTask.tag = evt.newValue);
        }


        private void RegisterFields(IReadOnlyList<object> fieldProperties)
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

                    case BlackboardBasedCondition: _fieldContainer.Add(new BBBasedConditionListField(property)); break;
                }
            }
        }
    }
}