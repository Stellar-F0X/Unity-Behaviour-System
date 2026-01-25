using UnityEngine;

namespace TaskStreamer.Tool
{
	[SearchTreeEntryName("New Action State")]
	internal class CreateNewStateScriptCommand : ScriptCreationCommandBase, ICommand
	{
		public CreateNewStateScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }


		public void Execute()
		{
			TSEditor.Instance.creationPopup.Open("State", CreateNewActionStateScript);
		}

		
		private void CreateNewActionStateScript(string name) 
		{
			TSEditorUtility.CreateNewNodeScript<FSMNodeScriptCreationCallback>(this, "NewActionState.cs", name);
		}
	}
}
