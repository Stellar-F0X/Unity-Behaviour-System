using System;
using Unity.Properties;
using UnityObject = UnityEngine.Object;

namespace TaskStreamer.Runtime
{
	[Serializable, GeneratePropertyBag, TaskDescription]
	public class UnityObjectCompare : Condition
	{
		public BlackboardVariable<UnityObject> firstObject;
		public BlackboardVariable<UnityObject> secondObject;
		
		public override bool Execute(NodeBase calledNode)
		{
			return firstObject.value == secondObject.value;
		}
	}
}