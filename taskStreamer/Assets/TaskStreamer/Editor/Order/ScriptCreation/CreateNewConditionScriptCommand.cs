using TaskStreamer.Runtime.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	[SearchTreeEntryName("New Condition")]
	internal class CreateNewConditionScriptCommand : ScriptCreationCommandBase<AddConditionOnScriptCreatedCommand>
	{
		public CreateNewConditionScriptCommand(TaskGraphView view, UGUID bbConditionGuid, Vector2 position) : base(view, position)
		{
			_bbConditionGuid = bbConditionGuid;
		}


		private readonly UGUID _bbConditionGuid;


		protected override string categoryName
		{
			get { return "Condition"; }
		}
		

		protected override PendingScriptCreationData CreatePendingScriptCreationData(string name)
		{
			GraphElement focusedElement = TSEditor.Instance.inspectorView.focusedElement;
			string scriptAssetPath = TSEditorUtility.CreateScriptFile("NewCondition.cs", name);
			
			bool success = this.TryGetTargetGuid(focusedElement, out UGUID targetGuid);
			Assert.IsTrue(success, $"{focusedElement.name}에서 대상 GUID를 가져올 수 없습니다.");

			PendingScriptCreationData data = new PendingScriptCreationData
			{
				scriptAssetPath = scriptAssetPath,
				graphGuid = view.focusGraph.guid,
				targetGuid = targetGuid,
				extraGuid = _bbConditionGuid,
				position = position,
				scriptName = name,
				focusOnCreated = true
			};

			return data;
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