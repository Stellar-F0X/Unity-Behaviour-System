using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	[SearchTreeEntryName("New Service")]
	internal class CreateNewServiceScriptCommand : ScriptCreationCommandBase<AddServiceOnScriptCreatedCommand>
	{
		public CreateNewServiceScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) {}
		

		protected override string categoryName
		{
			get { return "Service"; }
		}

		protected override PendingScriptCreationData CreatePendingScriptCreationData(string name)
		{
			NodeViewBase focusedNodeView = TSEditor.Instance.inspectorView.focusedElement as NodeViewBase;
			Assert.IsNotNull(focusedNodeView, "선택된 노드가 없습니다.");
			string scriptAssetPath = TSEditorUtility.CreateScriptFile("NewService.cs", name);

			PendingScriptCreationData data = new PendingScriptCreationData
			{
				scriptName = name,
				scriptAssetPath = scriptAssetPath,
				graphGuid = view.focusGraph.guid,
				targetGuid = focusedNodeView.targetNode.guid,
				position = position,
				focusOnCreated = true
			};

			return data;
		}
	}
}