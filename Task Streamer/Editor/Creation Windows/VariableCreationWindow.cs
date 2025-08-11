using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace TaskStreamer.Tool
{
    public class VariableCreationWindow :  CreationWindowBase
    {
        private event Action<Variable> _createCallback;


        public void RegisterNodeCreationCallbackOnce(Action<Variable> callback)
        {
            _createCallback = null;
            _createCallback = callback;
        }
        
        
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeList = new List<SearchTreeEntry>();
            Action<Type> creationAction = type => this.CreateVariable(type);
            searchTreeList.AddRange(this.CreateSearchTreeEntry<Variable>("Variables", creationAction, 0));
            return searchTreeList;
        }
        
        
        private Variable CreateVariable(Type variableType)
        {
            if (TaskStreamerEditor.canEditGraph == false)
            {
                Debug.LogError($"{nameof(VariableCreationWindow)} Error : CanEditGraph is false");
                return null;
            }

            Variable variable = TaskStreamerUtility.CreateVariable(variableType);
            _createCallback?.Invoke(variable);
            _createCallback = null;
            return variable;
        }
    }
}