using System;
using TaskStreamer.Runtime.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	internal class ConditionScriptCreationFactoryModule : ScriptCreationFactoryModule<CreateNewConditionScriptCommand>
	{
		public ConditionScriptCreationFactoryModule(TaskGraphView view, UGUID addToTargetGuid, string title, int layer = 1) : base(view, title, layer)
		{
			_bbConditionGuid = addToTargetGuid;
		}


		private readonly UGUID _bbConditionGuid;


		protected override CreateNewConditionScriptCommand Create(Type type, Vector2 position, string entryName)
		{
			Assert.IsFalse(_bbConditionGuid.IsEmpty(), $"{nameof(TaskGraphView)} is null");
			TaskGraphView arg1 = _view;
			UGUID arg2 = _bbConditionGuid;
			Vector2 arg3 = position;

			object createdObject = Activator.CreateInstance(type, arg1, arg2, arg3);
			CreateNewConditionScriptCommand command = createdObject as CreateNewConditionScriptCommand;
			command!.Execute();
			return command;
		}
	}
}