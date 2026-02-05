using UnityEngine;

namespace TaskStreamer.Tool
{
	internal abstract class CreateNewBTNodeScriptCommand : ScriptCreationCommandBase<AddBTNodeOnScriptCreatedCommand>
	{
		protected CreateNewBTNodeScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }

		protected abstract string scriptTemplateFullName
		{
			get;
		}

		
		protected override PendingScriptCreationData CreatePendingScriptCreationData(string name)
		{
			string path = TSEditorUtility.CreateScriptFile(scriptTemplateFullName, name);
			
			return new PendingScriptCreationData()
			{
				graphGuid = view.focusGraph.guid,
				scriptAssetPath = path,
				focusOnCreated = true,
				position = position,
				scriptName = name,
			};
		}
	}


	[SearchTreeEntryName("New Action Node")]
	internal class CreateNewActionNodeScriptCommand : CreateNewBTNodeScriptCommand
	{
		public CreateNewActionNodeScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }

		protected override string scriptTemplateFullName
		{
			get { return "NewActionNode.cs"; }
		}

		protected override string categoryName
		{
			get { return "Action"; }
		}
	}


	[SearchTreeEntryName("New Decorator Node")]
	internal class CreateNewDecoratorNodeScriptCommand : CreateNewBTNodeScriptCommand
	{
		public CreateNewDecoratorNodeScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }

		protected override string scriptTemplateFullName
		{
			get { return "NewDecoratorNode.cs"; }
		}
		
		protected override string categoryName
		{
			get { return "Decorator"; }
		}
	}


	[SearchTreeEntryName("New Composite Node")]
	internal class CreateNewCompositeNodeScriptCommand : CreateNewBTNodeScriptCommand
	{
		public CreateNewCompositeNodeScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }

		protected override string scriptTemplateFullName
		{
			get { return "NewCompositeNode.cs"; }
		}
		
		protected override string categoryName
		{
			get { return "Composite"; }
		}
	}
}
