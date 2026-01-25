using System;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	/// <summary> BT 노드(Action, Decorator, Composite) 스크립트 생성 완료 후 처리를 담당하는 콜백 </summary>
	internal class BTNodeScriptCreationCallback : IScriptCreationCompletedCallback
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
