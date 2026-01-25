using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	[SearchTreeEntryName("New Service")]
	internal class CreateNewServiceScriptCommand : ScriptCreationCommandBase, ICommand
	{
		public CreateNewServiceScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) {}


		public void Execute()
		{
			TSEditor.Instance.creationPopup.Open("Service", this.CreateServiceScript);
		}


		private void CreateServiceScript(string name)
		{
			NodeViewBase focusedNodeView = TSEditor.Instance.inspectorView.focusedElement as NodeViewBase;
			Assert.IsNotNull(focusedNodeView, "선택된 노드가 없습니다.");

			TSEditorUtility.CreateNewServiceScript<ServiceScriptCreationCallback>(this, focusedNodeView, "NewService.cs", name);
		}
	}
}