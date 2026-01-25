using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	internal class ScriptCreationFactoryModule<T> : FactoryModule<T> where T : class, ICommand
	{
		public ScriptCreationFactoryModule(TaskGraphView view, string title, int layer = 1) : base(typeof(T), title, false, layer)
		{
			_view = view;
		}
		
		
		protected readonly TaskGraphView _view;
		
		
		protected override T Create(Type type, Vector2 position, string entryName)
		{
			Assert.IsTrue(_view != null, $"{nameof(TaskGraphView)} is null");
			TaskGraphView arg1 = _view;
			Vector2 arg2 = position;

			T command = Activator.CreateInstance(type, arg1, arg2) as T;
			command!.Execute();
			return command;
		}
	}
}