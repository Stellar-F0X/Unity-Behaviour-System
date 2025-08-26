using System;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class SharedBBVariableFactoryModule : FactoryModule<ISharedBlackboardVariable>
    {
        public SharedBBVariableFactoryModule(Type targetType, string title, bool useCallback = false, int layer = 1) : base(targetType, title, useCallback, layer) { }


        protected override ISharedBlackboardVariable Create(Type type, Vector2 position, string bbVariableKey)
        {
            BlackboardAsset blackboardAsset = TaskStreamerEditor.Instance.graphAsset.blackboard;
            Debug.Assert(blackboardAsset is not null, "blackboardAsset is not null");

            BlackboardVariable bbVariable = blackboardAsset.FindVariable(bbVariableKey);
            Debug.Assert(bbVariable is not null, "bbVariable is not null");

            BlackboardVariable sharedBBVariable = ObjectFactory.CreateSharedBBVariable(blackboardAsset, bbVariable.guid, type);
            Debug.Assert(sharedBBVariable is ISharedBlackboardVariable, "sharedBBVariable is not null");

            return (ISharedBlackboardVariable)sharedBBVariable;
        }
    }
}