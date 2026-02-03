using System;
using System.Collections.Generic;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime
{
	[Serializable, GeneratePropertyBag, Readable]
	internal sealed partial class BlackboardBasedCondition
	{
		[DontCreateProperty]
		public EvaluationPolicy evaluationPolicy = EvaluationPolicy.All;

		[SerializeReference]
		public List<Condition> modules = new List<Condition>();

		[SerializeField, DontCreateProperty]
		private UGUID _guid = UGUID.Create();


		public UGUID guid
		{
			get { return _guid; }
		}



		public bool Execute(NodeBase calledNode)
		{
			if (modules is null)
			{
				Debug.LogWarning("Blackboard variables is not set for this condition.");
				return false;
			}

			if (modules.Count == 0)
			{
				return false;
			}

			switch (evaluationPolicy)
			{
				case EvaluationPolicy.Any: return this.EvaluateWithOrLogic(calledNode, modules.Count);

				case EvaluationPolicy.All: return this.EvaluateWithAndLogic(calledNode, modules.Count);
			}

			return false;
		}


		private bool EvaluateWithOrLogic(NodeBase calledNode, int count)
		{
			for (int index = 0; index < count; ++index)
			{
				if (this.modules[index].enable == false)
				{
					continue;
				}
				
				if (this.modules[index].Execute(calledNode))
				{
					return true;
				}
			}

			return false;
		}


		private bool EvaluateWithAndLogic(NodeBase calledNode, int count)
		{
			for (int index = 0; index < count; ++index)
			{
				if (this.modules[index].enable == false)
				{
					continue;
				}
				
				if (this.modules[index].Execute(calledNode) == false)
				{
					return false;
				}
			}

			return true;
		}
	}
}