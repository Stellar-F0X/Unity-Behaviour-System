using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class ConditionCreationWindow : CreationWindowBase
    {
        private event Action<ConditionModule> _createCallback;
        
        public void RegisterNodeCreationCallbackOnce(Action<ConditionModule> callback)
        {
            _createCallback = null;
            _createCallback = callback;
        }
        
        
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTree = new List<SearchTreeEntry>();
            Action<Type> creationAction = type => this.CreateConditionModule(type);
            searchTree.AddRange(this.CreateSearchTreeEntry<ConditionModule>("Conditions", creationAction, 0));
            return searchTree;
        }


        public ConditionModule CreateConditionModule(Type conditionType)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                Debug.LogError($"{nameof(VariableCreationWindow)} Error : CanEditGraph is false");
                return null;
            }

            ConditionModule variable = Activator.CreateInstance(conditionType) as ConditionModule;
            _createCallback?.Invoke(variable);
            _createCallback = null;
            return variable;
        }
    }
}