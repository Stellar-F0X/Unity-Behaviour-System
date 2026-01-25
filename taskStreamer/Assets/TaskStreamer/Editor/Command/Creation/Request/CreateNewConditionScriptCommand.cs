using TaskStreamer.Runtime.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	[SearchTreeEntryName("New Condition")]
	internal class CreateNewConditionScriptCommand : ScriptCreationCommandBase, ICommand
	{
		public CreateNewConditionScriptCommand(TaskGraphView view, UGUID bbConditionGuid, Vector2 position) : base(view, position)
		{
			_bbConditionGuid = bbConditionGuid;
		}


		private readonly UGUID _bbConditionGuid;


		public void Execute()
		{
			TSEditor.Instance.creationPopup.Open("Condition", this.CreateConditionScript);
		}


		private void CreateConditionScript(string name)
		{
			GraphElement focusedElement = TSEditor.Instance.inspectorView.focusedElement;
			bool success = this.TryGetTargetGuid(focusedElement, out UGUID targetGuid);
			Assert.IsTrue(success, $"{focusedElement.name}에서 대상 GUID를 가져올 수 없습니다.");

			TSEditorUtility.CreateNewConditionScript<ConditionScriptCreationCallback>(this, targetGuid, _bbConditionGuid, "NewCondition.cs", name);
		}


		private bool TryGetTargetGuid(GraphElement element, out UGUID guid)
		{
			switch (element)
			{
				case NodeViewBase nodeView: guid = nodeView.targetNode.guid; return true;

				case FSMEdge edge: guid = edge.targetTransition.guid; return true;
			}

			guid = UGUID.Empty;
			return false;
		}
	}
}