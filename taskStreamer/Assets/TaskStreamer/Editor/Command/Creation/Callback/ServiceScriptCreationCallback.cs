using System;
using TaskStreamer.Runtime.Utility;
using UnityEngine;
using UnityEngine.Assertions;

namespace TaskStreamer.Tool
{
	/// <summary> Service 스크립트 생성 완료 후 처리를 담당하는 콜백 </summary>
	internal class ServiceScriptCreationCallback : IScriptCreationCompletedCallback
	{
		public void OnScriptCreated(Type createdScriptType, PendingScriptCreationData data)
		{
			BehaviorNodeView view = TSEditor.Instance.taskGraphView.GetElementByGuid(data.targetGuid.ToString()) as BehaviorNodeView;
			Assert.IsNotNull(view,$"서비스를 추가할 노드를 찾을 수 없습니다: {data.targetGuid}");

			TSEditor.Instance.taskGraphView.SelectNodeForCode(view);
			view.observableServiceList.Add(TSObjectFactory.CreateService(createdScriptType));
			TSEditor.Instance.inspectorView.RefreshInspector();
		}
	}
}
