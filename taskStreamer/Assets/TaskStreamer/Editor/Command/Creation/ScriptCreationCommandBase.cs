using UnityEngine;

namespace TaskStreamer.Tool
{
	internal abstract class ScriptCreationCommandBase
	{
		protected ScriptCreationCommandBase(TaskGraphView view, Vector2 position)
		{
			this.position = position;
			this.view = view;
		}


		public TaskGraphView view
		{
			get;
			private set;
		}


		public Vector2 position
		{
			get;
			private set;
		}
	}
}
