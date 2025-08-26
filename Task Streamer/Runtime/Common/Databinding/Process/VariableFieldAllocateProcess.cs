using System;
using TaskStreamer.Utility;
using Unity.Properties;

namespace TaskStreamer.Injection
{
    //에디터에서 노드가 생성될때 쓰이는 PropertyVisitor로 필드의 BlackboardVariable을 할당한다.
    public class VariableFieldAllocateProcess : IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            Type valueType = context.Property.DeclaredValueType();

            if (valueType.IsInterface || valueType.IsAbstract)
            {
                return;
            }

            string name = context.Property.Name; //일단은 모두 Local Variable 필드로 생성한다.
            
            SetValueAttribute setValue = context.Property.GetAttribute<SetValueAttribute>();
            
            BlackboardVariable bbVariable = ObjectFactory.CreateBlackboardVariable(valueType, name, setValue?.defaultValue);

            if (bbVariable == null)
            {
                return;
            }

            context.Property.SetValue(ref container, bbVariable);
        }
    }
}