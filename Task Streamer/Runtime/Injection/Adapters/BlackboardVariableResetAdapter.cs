using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    public class BlackboardVariableResetAdapter : IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public BlackboardVariableResetAdapter(GraphVisitor visitor)
        {
            this._visitor = visitor;
        }

        private readonly GraphVisitor _visitor;


        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            Debug.Log(context.Property.Name);
            
            if (value is null || value.isGlobal == false)
            {
                return;

            }

            BlackboardVariable blackboardVariable = value.Clone();

            blackboardVariable.variable = null;

            context.Property.SetValue(ref container, blackboardVariable);
        }
    }
}