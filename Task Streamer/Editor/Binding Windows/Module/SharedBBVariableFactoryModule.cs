using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class SharedBBVariableFactoryModule : FactoryModule<ISharedBlackboardVariable>
    {
        public SharedBBVariableFactoryModule(string title, bool useCallback = false, int layer = 1) : base(typeof(BlackboardVariable), title, useCallback, layer) { }


        protected override ISharedBlackboardVariable Create(Type type, Vector2 position, string bbVariableKey)
        {
            BlackboardAsset blackboardAsset = TaskStreamerEditor.Instance.graphAsset.blackboard;
            Debug.Assert(blackboardAsset is not null, "Blackboard Asset is not null");

            BlackboardVariable bbVariable = blackboardAsset.FindVariable(bbVariableKey);
            Debug.Assert(bbVariable is not null, "blackboard Variable is not null");
            
            BlackboardVariable createdNewSharedVariable = this.CreateSharedBlackboardVariable(blackboardAsset, bbVariable, type);
            Debug.Assert(createdNewSharedVariable is ISharedBlackboardVariable, "Failed shared variable creation");

            return (ISharedBlackboardVariable)createdNewSharedVariable;
        }
        
        
        private BlackboardVariable CreateSharedBlackboardVariable(IBlackboard blackboard, BlackboardVariable variable, Type variableType)
        {
            BlackboardVariable bbVariable = ObjectFactory.CreateBlackboardVariable(variableType, shared: true);
            ISharedBlackboardVariable sharedVariable = bbVariable as ISharedBlackboardVariable;
            Debug.Assert(sharedVariable is not null, $"{variableType.Name} type is cannot be ISharedBlackboardVariable");
            
            sharedVariable.SetBlackboardAndVariableReference(blackboard, variable.guid);
            bbVariable.boxedValue = variable.boxedValue;
            return bbVariable;
        }
    }
}