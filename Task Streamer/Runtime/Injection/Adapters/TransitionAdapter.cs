using System.Collections.Generic;
using TaskStreamer.FSM;
using Unity.Properties;
using UnityEngine;

namespace TaskStreamer.Injection
{
    public class TransitionAdapter : IVisitPropertyAdapter<Transition>, IVisitPropertyAdapter<List<Transition>>
    {
        public TransitionAdapter(GraphVisitor visitor)
        {
            _visitor = visitor;
        }
        
        private readonly GraphVisitor _visitor;
        
        
        public void Visit<TContainer>(in VisitContext<TContainer, Transition> context, ref TContainer container, ref Transition value)
        {
            if (_visitor.debug)
            {
                Debug.Log($"{context.Property.Name}  Name: {value.name}  Des: {value.description}");
            }
            
            List<ConditionModule> conditions = value.conditions.modules;
            
            IPropertyBag<List<ConditionModule>> bag = PropertyBag.GetPropertyBag<List<ConditionModule>>();
            
            bag.Accept(_visitor, ref conditions);
        }

        
        public void Visit<TContainer>(in VisitContext<TContainer, List<Transition>> context, ref TContainer container, ref List<Transition> value)
        {
            List<Transition> runtimeTransitions = new List<Transition>(value.Count);

            int transitionCount = value.Count;
            
            for (int i = 0; i < transitionCount; i++)
            {
                runtimeTransitions.Add(Object.Instantiate(value[i]));
            }

            value = runtimeTransitions;
            
            context.ContinueVisitation(ref container, ref value);
        }
    }
}