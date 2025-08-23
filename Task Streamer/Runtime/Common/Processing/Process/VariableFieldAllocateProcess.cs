using System;
using TaskStreamer.Utility;
using Unity.Properties;

namespace TaskStreamer.Injection
{
    //[EditorOnly]
    public class VariableFieldAllocateProcess : IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            SetValueAttribute setValue = context.Property.GetAttribute<SetValueAttribute>();
            Type valueType = context.Property.DeclaredValueType();
            
            context.Property.SetValue(ref container, TSObjectFactory.CreateBlackboardVariable(valueType, setValue?.defaultValue, true));
        }
    }
}