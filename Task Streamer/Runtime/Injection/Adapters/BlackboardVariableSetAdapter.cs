using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    public class BlackboardVariableSetAdapter : IVisitContravariantPropertyAdapter<BlackboardVariable>
    {
        public BlackboardVariableSetAdapter(GraphVisitor visitor)
        {
            this._visitor = visitor;
        }
        
        private readonly GraphVisitor _visitor;
        
        
        public void Visit<TContainer>(in VisitContext<TContainer> context, ref TContainer container, BlackboardVariable value)
        {
            if (value is null || _visitor.blackboard == null || _visitor.blackboard.variables.Count == 0)
            {
                return;
            }

            if (value.isGlobal)
            {
                Variable foundVariable = _visitor.blackboard.FindVariable(value.name);

                // Error: The specified variable was not found in the blackboard.
                Debug.Assert(foundVariable != null, "Variable not found in blackboard.");

                BlackboardVariable blackboardVariable = value.Clone();
                blackboardVariable.variable = foundVariable;
                
                context.Property.SetValue(ref container, blackboardVariable);
            }
            else
            {
                BlackboardVariable blackboardVariable = value.Clone();
            
                context.Property.SetValue(ref container, blackboardVariable);
            }
            
            if (_visitor.debug)
            {
                Debug.Log($"{context.Property.Name} {value.name}({value.isGlobal})");
            }
        }
    }
}