using System;
using NUnit.Framework;

namespace TaskStreamer.Tool
{
	/// <summary> FSM State 스크립트 생성 완료 후 처리를 담당하는 콜백 </summary>
	internal class FSMNodeScriptCreationCallback : IScriptCreationCompletedCallback
	{
		public void OnScriptCreated(Type createdScriptType, PendingScriptCreationData data)
		{
			NodeViewBase view = TSEditor.Instance.taskGraphView.CreateNewNodeAndView(createdScriptType, data.position);
			Assert.IsNotNull(view, "Node view is null");

			if (data.focusOnCreated)
			{
				TSEditor.Instance.taskGraphView.SelectNodeForCode(view);
			}
		}
	}
}
