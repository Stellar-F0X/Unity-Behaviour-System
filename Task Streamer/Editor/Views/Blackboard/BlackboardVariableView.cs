using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
    /// <summary>
    /// Represents a UI element for displaying and editing a variable in the blackboard within a Unity editor tool.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="VisualElement"/> and provides the UI and functionality for managing a single variable,
    /// including its name, type, and value. It also includes event handling for delete requests and name changes.
    /// </remarks>
    public class BlackboardVariableView : VisualElement
    {
        /// Represents a view in the Blackboard interface that allows rendering and interaction with a Blackboard variable.
        /// This class extends `VisualElement` and provides user interface elements for managing variables in the editor.
        public BlackboardVariableView()
        {
            TaskStreamerEditor.settings.bbVariableXml.CloneTree(this);

            _deleteButton = this.Q<Button>("delete-button");
            _nameField = this.Q<TextField>("name-field");
            _imguiField = this.Q<IMGUIContainer>("imgui-field");
            _typeNameLabel = this.Q<Label>("type-name-label");

            _deleteButton.UnregisterCallback<ClickEvent>(this.OnDeleteButtonClicked);
            _deleteButton.RegisterCallback<ClickEvent>(this.OnDeleteButtonClicked);
            
            _nameField.UnregisterCallback<FocusOutEvent>(this.OnNameFieldFocusOut);
            _nameField.RegisterCallback<FocusOutEvent>(this.OnNameFieldFocusOut);
        }

        /// <summary>
        /// Represents the size of the icon used within the interface.
        /// This constant value ensures consistent dimensions for UI elements like error indicators or decorations.
        /// </summary>
        private const float _ICON_SIZE = 12f;

        /// <summary>
        /// Event triggered when a delete request is initiated
        /// for the associated blackboard variable UI element.
        /// </summary>
        public event Action<BlackboardVariableView> OnDeleteRequested;

        /// <summary>
        /// Event triggered when the key of a blackboard variable changes.
        /// </summary>
        public event Action<BlackboardVariableView, string> OnKeyChanged;

        /// <summary>
        /// Represents the label used to display the type name of a variable in the blackboard UI.
        /// </summary>
        private Label _typeNameLabel;

        /// <summary>
        /// Represents the delete button in the blackboard for handling user-initiated variable deletion.
        /// </summary>
        private Button _deleteButton;

        /// <summary>
        /// Represents a private <see cref="TextField"/> element used for managing or displaying
        /// the name of a variable in the blackboard UI.
        /// </summary>
        private TextField _nameField;

        /// <summary>
        /// Represents an IMGUIContainer used to render and manage custom IMGUI content
        /// within the blackboard variable view interface.
        /// </summary>
        private IMGUIContainer _imguiField;

        /// <summary>
        /// Represents the encapsulated variable within the BlackboardVariableView instance.
        /// </summary>
        private BlackboardVariable _variable;

        /// <summary>
        /// Represents the serialized property that stores the value associated with a variable in the blackboard.
        /// </summary>
        private SerializedProperty _valueProperty;


        /// <summary>
        /// Represents the blackboard variable associated with the current variable view.
        /// </summary>
        public BlackboardVariable variable
        {
            get { return _variable; }
        }


        /// <summary> Sets up the BlackboardVariableView with the provided Variable and SerializedProperty. </summary>
        /// <param name="bbVariable">The Variable instance containing the data to display in the BlackboardVariableView.</param>
        /// <param name="serializedProperty">The SerializedProperty used for binding and persisting data changes.</param>
        public void Setup(BlackboardVariable bbVariable, SerializedProperty serializedProperty)
        {
            _variable = bbVariable;

            if (bbVariable.implementedType.BaseType is null)
            {
                _typeNameLabel.text = bbVariable.implementedType.Name;
            }
            else
            {
                _typeNameLabel.text = bbVariable.implementedType.BaseType.GenericTypeArguments[0].Name;
            }

            if (serializedProperty is null)
            {
                return;
            }

            _valueProperty = serializedProperty;
            this.UpdateUI();
        }


        /// <summary>Updates the User Interface (UI) of the blackboard variable view based on the current state of the associated variable and editor settings.</summary>
        private void UpdateUI()
        {
            if (_variable is null)
            {
                return;
            }

            this.tooltip = _variable.implementedType.Name;

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


        /// <summary>Configures the IMGUIContainer to handle rendering and interaction for a serialized property.</summary>
        /// This method sets up the onGUIHandler for custom IMGUI logic, ensures proper binding to a serialized property, and schedules repaint tasks to maintain synchronization.
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


        /// <summary> Draws the IMGUI (Immediate Mode GUI) for the provided serialized property. </summary>
        /// <param name="valueProp">The serialized property object to be rendered. If null, an error message is displayed.</param>
        private void DrawIMGUIForItem(SerializedProperty valueProp)
        {
            if (valueProp is null)
            {
                Rect pos = EditorGUILayout.GetControlRect();
                Rect textRect = new Rect(pos.x + _ICON_SIZE + 2f, pos.y, pos.width - _ICON_SIZE - 2f, pos.height);
                VisualUtility.DrawError(textRect, "Invalid blackboard property type.", _ICON_SIZE);
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


        /// <summary> Handles the click event for the delete button. </summary>
        /// <param name="evt">The click event associated with the delete button.</param>
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


        /// <summary> Handles the event triggered when the name field loses focus. </summary>
        /// <param name="evt">The focus out event associated with the name field losing focus.</param>
        private void OnNameFieldFocusOut(FocusOutEvent evt)
        {
            if (_nameField is null || _variable is null)
            {
                return;
            }

            this.OnKeyChanged?.Invoke(this, _nameField.value);
            this._nameField.SetValueWithoutNotify(_variable.key);
        }


        /// <summary>Creates a scheduled item that runs an action at a specified interval.</summary>
        /// <param name="intervalMs">The interval in milliseconds to execute the scheduled action.</param>
        /// <returns>An <see cref="IVisualElementScheduledItem"/> representing the scheduled operation.</returns>
        private IVisualElementScheduledItem CreateScheduledItem(int intervalMs)
        {
            return this.schedule.Execute(base.MarkDirtyRepaint)
                       .Until(this.IsContinuable)
                       .Every(intervalMs);
        }


        /// <summary>
        /// Determines whether the current operation or condition is continuable based on the play mode state and variable validity.
        /// </summary>
        /// <returns>True if the operation can continue; otherwise, false.</returns>
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

            if (_variable.implementedType is null)
            {
                return false;
            }

            return true;
        }
    }
}