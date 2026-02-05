using UnityEngine;

namespace TaskStreamer.Tool
{
	[SearchTreeEntryName("New Action State")]
	internal class CreateNewStateScriptCommand : ScriptCreationCommandBase<AddFSMNodeOnScriptCreatedCommand>
	{
		public CreateNewStateScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }

		protected override string categoryName
		{
			get { return "State"; }
		}

		protected override PendingScriptCreationData CreatePendingScriptCreationData(string name)
		{
			string scriptAssetPath = TSEditorUtility.CreateScriptFile("NewActionState.cs", name);

			return new PendingScriptCreationData
			{
				scriptAssetPath = scriptAssetPath,
				graphGuid = view.focusGraph.guid,
				position = position,
				scriptName = name,
				focusOnCreated = true
			};
		}
	}
}
