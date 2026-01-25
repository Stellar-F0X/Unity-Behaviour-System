using UnityEngine;

namespace TaskStreamer.Tool
{
	internal abstract class CreateNewBTNodeScriptCommandBase : ScriptCreationCommandBase, ICommand
	{
		protected CreateNewBTNodeScriptCommandBase(TaskGraphView view, Vector2 position) : base(view, position) { }


		/// <summary> 노드 생성 Window를 호출 </summary>
		public abstract void Execute();
	}


	[SearchTreeEntryName("New Action Node")]
	internal class CreateNewActionNodeScriptCommand : CreateNewBTNodeScriptCommandBase
	{
		public CreateNewActionNodeScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }

		public override void Execute()
		{
			TSEditor.Instance.creationPopup.Open("Action", this.CreateNewActionNodeScript);
		}

		private void CreateNewActionNodeScript(string name) 
		{
			TSEditorUtility.CreateNewNodeScript<BTNodeScriptCreationCallback>(this, "NewActionNode.cs", name);
		}
	}


	[SearchTreeEntryName("New Decorator Node")]
	internal class CreateNewDecoratorNodeScriptCommand : CreateNewBTNodeScriptCommandBase
	{
		public CreateNewDecoratorNodeScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }

		public override void Execute()
		{
			TSEditor.Instance.creationPopup.Open("Decorator", this.CreateNewDecoratorNodeScript);
		}

		private void CreateNewDecoratorNodeScript(string name) 
		{
			TSEditorUtility.CreateNewNodeScript<BTNodeScriptCreationCallback>(this, "NewDecoratorNode.cs", name);
		}
	}


	[SearchTreeEntryName("New Composite Node")]
	internal class CreateNewCompositeNodeScriptCommand : CreateNewBTNodeScriptCommandBase
	{
		public CreateNewCompositeNodeScriptCommand(TaskGraphView view, Vector2 position) : base(view, position) { }

		public override void Execute()
		{
			TSEditor.Instance.creationPopup.Open("Composite", CreateNewCompositeNodeScript);
		}

		private void CreateNewCompositeNodeScript(string name) 
		{
			TSEditorUtility.CreateNewNodeScript<BTNodeScriptCreationCallback>(this, "NewCompositeNode.cs", name);
		}
	}
}
