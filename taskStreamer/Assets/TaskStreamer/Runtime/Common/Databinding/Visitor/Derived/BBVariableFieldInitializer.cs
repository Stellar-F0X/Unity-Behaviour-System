using System;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime
{
#if UNITY_EDITOR
	/// <summary>
	/// 에디터에서 노드나 서비스, 조건(Condition)을 생성 시 BlackboardVariable 필드를 초기화합니다.
	/// DefaultValueAttribute가 있으면 해당 값으로, 없으면 기본값으로 Local Variable을 생성합니다.
	/// </summary>
	public class BBVariableFieldInitializer : ReadableVisitorBase, IVisitContravariantPropertyAdapter<BlackboardVariable>
	{
		public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
		{
			if (context.Property.IsReadOnly)
			{
				Debug.LogError($"'{typeof(TContainer)}.{context.Property.Name}' is read-only.");
				return;
			}

			Type valueType = context.Property.DeclaredValueType();

			if (valueType.IsInterface || valueType.IsAbstract)
			{
				return;
			}

			DefaultValueAttribute defaultAttr = context.Property.GetAttribute<DefaultValueAttribute>();
			BlackboardVariable variable = null;

			if (defaultAttr is null)
			{
				variable = TSObjectFactory.CreateBlackboardVariable(valueType, context.Property.Name);
			}
			else
			{
				variable = TSObjectFactory.CreateBlackboardVariable(valueType, context.Property.Name, defaultAttr.defaultValue);
			}

			if (variable != null)
			{
				context.Property.SetValue(ref container, variable);
			}
		}
	}
#endif
}