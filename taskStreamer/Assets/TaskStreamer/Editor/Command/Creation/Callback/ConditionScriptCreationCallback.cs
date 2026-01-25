using System;
using System.Linq;
using TaskStreamer.Runtime;
using TaskStreamer.Runtime.BT;
using TaskStreamer.Runtime.Utility;
using UnityEditor;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
	/// <summary> Condition 스크립트 생성 완료 후 처리를 담당하는 콜백 </summary>
	internal class ConditionScriptCreationCallback : IScriptCreationCompletedCallback
	{
		public void OnScriptCreated(Type createdScriptType, PendingScriptCreationData data)
		{
			VisualElement element = TSEditor.Instance.taskGraphView.GetElementByGuid(data.targetGuid.ToString());
			BlackboardBasedCondition bbCondition = this.GetBlackboardBasedCondition(element, data.extraGuid);
			Assert.IsNotNull(bbCondition, $"BlackboardBasedCondition을 찾을 수 없습니다: {data.extraGuid}");

			Condition newCondition = TSObjectFactory.CreateConditionModule(createdScriptType);
			Assert.IsNotNull(newCondition, $"Condition을 생성할 수 없습니다: {createdScriptType}");

			Undo.RecordObject(TSEditor.Instance.graphAsset, "TaskStreamer (AddCondition)");
			bbCondition.modules.Add(newCondition);
			EditorUtility.SetDirty(TSEditor.Instance.graphAsset);

			switch (element)
			{
				case FSMEdge edge: TSEditor.Instance.inspectorView.UpdateSelection(edge); break;

				case NodeViewBase nodeView: TSEditor.Instance.inspectorView.UpdateSelection(nodeView); break;
			}

			EditorApplication.delayCall += TSEditor.Instance.inspectorView.RefreshInspector;
		}


		private BlackboardBasedCondition GetBlackboardBasedCondition(VisualElement view, UGUID bbConditionGuid)
		{
			if (view is FSMEdge edge)
			{
				return edge.targetTransition.conditions;
			}

			if (view is not BehaviorNodeView nodeView)
			{
				return null;
			}

			if (nodeView.targetNode is BBBasedConditionNode bbConditionNode)
			{
				return bbConditionNode.conditions;
			}

			return ((BBBasedConditionService)nodeView.observableServiceList.FirstOrDefault(FindBBCS))?.conditions;
			
#region Local Function
			//Find Blackboard Based Condition Service
			bool FindBBCS(ServiceBase s) => s is BBBasedConditionService bbcs && bbcs.conditions.guid == bbConditionGuid;
#endregion
		}
	}
}
