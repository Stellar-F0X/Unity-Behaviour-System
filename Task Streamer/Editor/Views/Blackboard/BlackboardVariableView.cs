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
        /// This constant value is utilized to uniformly define the dimensions of icons,
        /// primarily in components like user interface elements or error displays.
        /// </summary>
        private const float _ICON_SIZE = 12f;

        /// <summary>
        /// An event that is triggered when a delete action is requested for a <see cref="BlackboardVariableView"/>.
        /// </summary>
        /// <remarks>
        /// This event is invoked when the delete button within the associated <see cref="BlackboardVariableView"/> is clicked.
        /// It passes the instance of the <see cref="BlackboardVariableView"/> that is requesting the delete action as an argument.
        /// This allows subscribers to the event to handle the delete request appropriately, such as removing the blackboard variable
        /// from a collection or updating the user interface.
        /// </remarks>
        /// <example>
        /// Use this event in conjunction with the <c>OnVariableDeleteRequested</c> handler in the <see cref="BlackboardView"/>
        /// class to respond to delete requests from individual variable views.
        /// </example>
        public event Action<BlackboardVariableView> OnDeleteRequested;

        /// <summary>
        /// An event triggered when the key associated with a <see cref="BlackboardVariableView"/> has changed.
        /// </summary>
        /// <remarks>
        /// The event provides the instance of <see cref="BlackboardVariableView"/> that triggered the change
        /// and the new key as a string. This allows handling updates to the key value of a blackboard variable,
        /// facilitating synchronization of the changes with other components or systems.
        /// </remarks>
        /// <param name="arg1">The <see cref="BlackboardVariableView"/> instance whose key has changed.</param>
        /// <param name="arg2">The new key value as a string.</param>
        public event Action<BlackboardVariableView, string> OnKeyChanged;

        /// <summary>
        /// Represents the label that displays the type name of the variable in the blackboard variable view.
        /// </summary>
        /// <remarks>
        /// This label is updated based on the variable's type provided when the blackboard variable view is set up.
        /// It is used to display the type name or base type name of the associated variable in the user interface.
        /// </remarks>
        private Label _typeNameLabel;

        /// <summary>
        /// Represents the delete button in the <see cref="BlackboardVariableView"/> class,
        /// used to trigger the deletion of a variable in the blackboard.
        /// </summary>
        /// <remarks>
        /// The delete button functionality is tied to the deletion workflow for variables. It is used to
        /// handle user interactions for deleting entries within the blackboard. The button's interactivity
        /// is managed based on the current edit permissions defined in <see cref="TaskStreamerEditor.canEditGraph"/>.
        /// </remarks>
        private Button _deleteButton;

        /// <summary>
        /// Represents a private <see cref="TextField"/> element used for managing or displaying the name
        /// of a variable in the UI. This field allows user interaction to modify the variable name
        /// and triggers corresponding focus-out events for handling name changes.
        /// </summary>
        private TextField _nameField;

        /// <summary>
        /// Represents an <see cref="IMGUIContainer"/> UI element used in the blackboard variable view
        /// for rendering custom IMGUI content related to a serialized property.
        /// </summary>
        /// <remarks>
        /// This field is configured through the <see cref="SetupIMGUIContainer"/> method to handle
        /// IMGUI-specific rendering behavior by assigning a custom GUI handler.
        /// It interacts with <see cref="SerializedProperty"/> for displaying and handling variable values.
        /// </remarks>
        private IMGUIContainer _imguiField;

        /// <summary>
        /// Represents the encapsulated variable associated with the current instance of the BlackboardVariableView component.
        /// This variable serves as a reference to a user-defined or custom data structure that is crucial for managing
        /// dynamic, serialized data within the system.
        /// </summary>
        /// <remarks>
        /// The <c>_variable</c> field is of type <c>Variable</c>, which includes core metadata like type information,
        /// unique identifiers (GUID), and the ability to return or accept boxed values. It supports deserialization
        /// and cloning operations for flexible data management.
        /// </remarks>
        private BlackboardVariable _variable;

        /// <summary>
        /// Represents a serialized property that holds the value associated with the current Variable instance.
        /// </summary>
        /// <remarks>
        /// This property is used during the setup and update of the UI elements within the BlackboardVariableView.
        /// It allows UI elements to bind to and interact with the serialized data of the associated Variable.
        /// </remarks>
        private SerializedProperty _valueProperty;


        /// Gets the associated `Variable` instance for this `BlackboardVariableView`.
        /// This property provides access to the underlying `Variable` that is represented
        /// and managed by this specific `BlackboardVariableView` instance. The `Variable` holds
        /// relevant data and logic pertinent to the application.
        public BlackboardVariable variable
        {
            get { return _variable; }
        }


        /// <summary>
        /// Sets up the BlackboardVariableView with the provided Variable and SerializedProperty.
        /// Populates the UI with data from the given Variable and binds the SerializedProperty to handle its updates.
        /// </summary>
        /// <param name="bbVariable">The Variable instance containing the data to display in the BlackboardVariableView.</param>
        /// <param name="serializedProperty">The SerializedProperty used for binding and persisting data changes.</param>
        public void Setup(BlackboardVariable bbVariable, SerializedProperty serializedProperty)
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


        /// <summary>
        /// Updates the User Interface (UI) of the blackboard variable view based on the current state of the associated variable and editor settings.
        /// </summary>
        /// <remarks>
        /// This method refreshes the UI elements of the blackboard variable view by setting their properties according to the state of the associated <see cref="Variable"/> instance and serialized property.
        /// It updates the tooltip, the name field, and the delete button, and initializes the IMGUI container for displaying the variable's value.
        /// </remarks>
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


        /// Configures the IMGUIContainer associated with a BlackboardVariableView instance to handle
        /// the display and interaction logic for a serialized property.
        /// This method binds the appropriate IMGUI event handlers to enable real-time interaction
        /// with the serialized value of a variable. It considers special cases such as null or
        /// uninitialized serialized properties to ensure robust functionality. Additionally, it
        /// schedules a repaint task to maintain synchronicity with any changes in the serialized value.
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


        /// <summary>
        /// Draws the IMGUI (Immediate Mode GUI) representation for a given serialized property of a Blackboard item.
        /// Useful for rendering custom UI elements within the Blackboard UI.
        /// </summary>
        /// <param name="valueProp">The serialized property object to be rendered in the IMGUI layout. If null, an error indicator will be displayed.</param>
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


        /// Handles the click event for the delete button.
        /// This method is executed when the delete button in the blackboard variable view
        /// is clicked. If editing the graph is not allowed or the variable is null, it exits early.
        /// Otherwise, it triggers the `OnDeleteRequested` event to notify subscribers about
        /// the delete action.
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


        /// <summary>
        /// Handles the event triggered when the name field loses focus.
        /// This method updates the associated variable's key based on the current value
        /// of the name field and ensures the UI reflects the correct value.
        /// </summary>
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


        /// <summary>
        /// Creates a scheduled item that executes a specified action at a fixed interval.
        /// </summary>
        /// <param name="intervalMs">The interval in milliseconds at which the action should be executed.</param>
        /// <returns>An instance of <see cref="IVisualElementScheduledItem"/> that represents the scheduled action.</returns>
        private IVisualElementScheduledItem CreateScheduledItem(int intervalMs)
        {
            return this.schedule.Execute(base.MarkDirtyRepaint)
                       .Until(this.IsContinuable)
                       .Every(intervalMs);
        }


        /// <summary>
        /// Determines whether the current operation or condition is continuable based on the editor's play mode state and the state of the associated variable.
        /// </summary>
        /// <returns>
        /// True if the operation can continue execution; otherwise, false.
        /// </returns>
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