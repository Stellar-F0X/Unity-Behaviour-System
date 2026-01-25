using System;
using System.Linq;
using TaskStreamer.Runtime.Utility;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
	public class ScriptCreationPopup : VisualElement
	{
		public ScriptCreationPopup()
		{
			TSEditor.nodeScriptCreationPopup.CloneTree(this);

			this.style.flexGrow = 1f;
			this.style.position = Position.Absolute;
			this.style.width = Length.Percent(100);
			this.style.height = Length.Percent(100);
			this.style.display = DisplayStyle.None;

			this._titleLabel = this.Q<Label>("title-label");
			this._closeButton = this.Q<Button>("close-button");
			this._createButton = this.Q<Button>("create-button");
			this._nameInputField = this.Q<TextField>("name-field");

			this._closeButton.clicked += this.Close;
			this._createButton.clicked += this.CreateNewNodeScript;
		}

        private readonly Label _titleLabel;
		private readonly TextField _nameInputField;
		private readonly Button _createButton;
		private readonly Button _closeButton;
		
		private Action<string> _creationCallback;


		public void Open(string subjectName, Action<string> creationCallback)
		{
			this._creationCallback = creationCallback;
			this._titleLabel.text = $"Create {subjectName}";
			this._nameInputField.label = $"New {subjectName} Name";
			
			this.style.display = DisplayStyle.Flex;
		}


		public void Close()
		{
			this._nameInputField.value = string.Empty;
			this.style.display = DisplayStyle.None;
		}


		private void CreateNewNodeScript()
		{
			if (StringUtility.IsValidScriptName(_nameInputField.text, out string message))
			{
				this._creationCallback.Invoke(_nameInputField.text);
				this.Close();
				return;
			}

			Debug.LogError(message);
		}
	}
}