using System;
using TaskStreamer.Runtime.Utility;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Runtime
{
#if UNITY_EDITOR
    /// <summary> 에디터에서 노드가 생성될때 쓰이는 PropertyVisitor로, 'Node 객체를 대상'으로 필드의 BlackboardVariable을 할당한다. </summary>
    public class BlackboardVariableFieldInitializeVisitor : ReadableVisitorBase, IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (context.Property.IsReadOnly)
            {
                Debug.LogError($"'{typeof(TContainer)}.{context.Property.Name}' is read-only and cannot be modified.");
                return;
            }

            Type valueType = context.Property.DeclaredValueType();

            if (valueType.IsInterface || valueType.IsAbstract)
            {
                return;
            }

            BlackboardVariable bbVariable = null;

            DefaultValueAttribute setValue = context.Property.GetAttribute<DefaultValueAttribute>();

            if (setValue is not null)
            {
                //일단은 모두 Local Variable 필드로 생성한다.
                bbVariable = ObjectFactory.CreateBlackboardVariable(valueType, context.Property.Name, setValue.defaultValue);
            }
            else
            {
                //일단은 모두 Local Variable 필드로 생성한다.
                bbVariable = ObjectFactory.CreateBlackboardVariable(valueType, context.Property.Name);
            }

            if (bbVariable == null)
            {
                return;
            }

            context.Property.SetValue(ref container, bbVariable);
        }
    }
#endif
}