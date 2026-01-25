using System;
using TaskStreamer.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class SharedBBVariableProvider : ICategoryTreeProvider
    {
        public SharedBBVariableProvider(Type bindTargetVariableType)
        {
            if (bindTargetVariableType is null)
            {
                Debug.LogError("Bind Target Variable Type is null");
                return;
            }

            if (typeof(BlackboardVariable).IsAssignableFrom(bindTargetVariableType) == false)
            {
                Debug.LogError($"Invalid type binding: {bindTargetVariableType.Name} should not be assignable from BlackboardVariable");
                return;
            }
            
            this._bindTargetVariableType = bindTargetVariableType;
        }


        private readonly Type _bindTargetVariableType;
        
        
        public SearchTreeEntry[] ProvideCategories(FactoryModule module)
        {
            Debug.Assert(TSEditor.canEditGraph, "Cannot edit graph");
            BlackboardAsset blackboard = TSEditor.Instance.graphAsset.blackboard;
            Debug.Assert(blackboard != null, "Blackboard cannot be null");

            
            BlackboardVariable[] variables = blackboard.GetVariablesByType(_bindTargetVariableType);
            
            SearchTreeEntry[] entries = new SearchTreeEntry[variables.Length + 1];
            entries[0] = new SearchTreeGroupEntry(new GUIContent(module.title));
            entries[0].level = module.layer;

            for (int i = 1; i < entries.Length; ++i)
            {
                BlackboardVariable bbVariable = variables[i - 1];
                entries[i] = new SearchTreeEntry(new GUIContent(bbVariable.key));
                entries[i].userData = (bbVariable.genericVariableType, module);
                entries[i].level = module.layer + 1;
            }

            return entries;
        }
    }
}