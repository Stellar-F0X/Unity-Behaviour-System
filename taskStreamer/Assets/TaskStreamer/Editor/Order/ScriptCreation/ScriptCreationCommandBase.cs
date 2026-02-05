using TaskStreamer.Runtime;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	internal abstract class ScriptCreationCommandBase<TCallback> : ICommand where TCallback : IScriptCreationCompletedCallback, new()
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

		protected abstract string categoryName
		{
			get; 
		}
		
		
		
		public void Execute()
		{
			Assert.IsNotNull(TSEditor.Instance, $"TSEditor is null reference.");
			TSEditor.Instance.creationPopup.Open(categoryName, this.CreateNewScript);
		}
		
		
		
		private void CreateNewScript(string name)
		{
			PendingCreationHandler.RequestScriptCreation<TCallback>(this.CreatePendingScriptCreationData(name));
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
			CompilationPipeline.RequestScriptCompilation();
			EditorUtility.RequestScriptReload();
		}
		
		
		
		protected abstract PendingScriptCreationData CreatePendingScriptCreationData(string name);
	}
}
