using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    public class BlackboardVariableView : VisualElement
    {
        public BlackboardVariableView()
        {
            TaskStreamerEditor.settings.blackboardVariableViewXml.CloneTree(this);

            _deleteButton = this.Q<Button>("delete-button");
            _nameField = this.Q<TextField>("name-field");
            _imguiField = this.Q<IMGUIContainer>("imgui-field");
            _typeNameLabel = this.Q<Label>("type-name-label");

            _deleteButton?.RegisterCallback<ClickEvent>(this.OnDeleteButtonClicked);
            _nameField?.RegisterCallback<FocusOutEvent>(this.OnNameFieldFocusOut);
        }

        private const float _ICON_SIZE = 12f;
        
        public event Action<BlackboardVariableView> OnDeleteRequested;

        public event Action<BlackboardVariableView, string> OnKeyChanged;
        
        private Label _typeNameLabel;
        private Button _deleteButton;
        private TextField _nameField;
        private IMGUIContainer _imguiField;
        
        private Variable _variable;
        private SerializedProperty _valueProperty;


        public Variable variable
        {
            get { return _variable; }
        }


        public void Setup(Variable bbVariable, SerializedProperty serializedProperty)
        {
            _variable = bbVariable;

            if (bbVariable.type.BaseType is null)
            {
                _typeNameLabel.text = bbVariable.type.Name;
            }
            else
            {
                _typeNameLabel.text = bbVariable.type.BaseType.GenericTypeArguments[0].Name;
            }

            if (serializedProperty is null)
            {
                return;
            }

            _valueProperty = serializedProperty;
            this.UpdateUI();
        }


        private void UpdateUI()
        {
            if (_variable is null)
            {
                return;
            }

            this.tooltip = _variable.type.Name;

            if (_nameField is not null)
            {
                _nameField.SetValueWithoutNotify(_variable.key);
                _nameField.enabledSelf = TaskStreamerEditor.canEditGraph;
            }

            if (_deleteButton is not null)
            {
                _deleteButton.enabledSelf = TaskStreamerEditor.canEditGraph;
            }

            this.SetupIMGUIContainer();
        }

        
        private void SetupIMGUIContainer()
        {
            if (_imguiField is null || SerializedProperty.DataEquals(_valueProperty, null))
            {
                return;
            }

            if (_valueProperty.boxedValue is null)
            {
                return;
            }

            SerializedProperty valueProp = _valueProperty.FindPropertyRelative("_value");

            if (SerializedProperty.DataEquals(valueProp, null))
            {
                return;
            }
            
            _imguiField.Unbind();
            _imguiField.onGUIHandler = () => this.DrawIMGUIForItem(valueProp);

            IVisualElementScheduledItem scheduled = this.CreateScheduledItem(250);
            _imguiField.RegisterCallbackOnce<DetachFromPanelEvent>(_ => scheduled.Pause());
        }

        
        private void DrawIMGUIForItem(SerializedProperty valueProp)
        {
            if (valueProp is null)
            {
                Rect pos = EditorGUILayout.GetControlRect();
                Rect textRect = new Rect(pos.x + _ICON_SIZE + 2f, pos.y, pos.width - _ICON_SIZE - 2f, pos.height);
                EditorUtilities.DrawError(textRect, "Invalid blackboard property type.", _ICON_SIZE);
            }
            else
            {
                GUI.enabled = !Application.isPlaying;
                valueProp.serializedObject.Update();
                EditorGUILayout.PropertyField(valueProp, GUIContent.none, true);
                valueProp.serializedObject.ApplyModifiedProperties();
                GUI.enabled = true;
            }
        }


        private void OnDeleteButtonClicked(ClickEvent evt)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                return;
            }

            if (_variable is null)
            {
                return;
            }

            this.OnDeleteRequested?.Invoke(this);
        }


        private void OnNameFieldFocusOut(FocusOutEvent evt)
        {
            if (_nameField is null || _variable is null)
            {
                return;
            }

            this.OnKeyChanged?.Invoke(this, _nameField.value);
            this._nameField.SetValueWithoutNotify(_variable.key);
        }


        private IVisualElementScheduledItem CreateScheduledItem(int intervalMs)
        {
            return this.schedule.Execute(base.MarkDirtyRepaint)
                       .Until(this.IsContinuable)
                       .Every(intervalMs);
        }


        private bool IsContinuable()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return false;
            }

            if (_variable is null || _variable is null)
            {
                return false;
            }
            
            if (_variable.type is null)
            {
                return false;
            }

            return true;
        }
    }
}