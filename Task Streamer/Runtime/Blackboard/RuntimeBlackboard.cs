using System;
using System.Collections.Generic;
using TaskStreamer.Utility;
using UnityEngine;

namespace TaskStreamer
{
    [Serializable]
    internal class RuntimeBlackboard : IBlackboard
    {
        private bool _isInitialized;
        
        [SerializeField]
        private UGUID _appliedVersion;

        [SerializeReference]
        private List<Variable> _clonedVariables = new List<Variable>();

        private readonly Dictionary<string, Variable> _runtimeVariables = new Dictionary<string, Variable>();



        public IReadOnlyList<Variable> variables
        {
            get { return _clonedVariables; }
        }

        public int count
        {
            get { return _clonedVariables is null ? 0 : _clonedVariables.Count; }
        }

        public bool isInitialized
        {
            get { return _isInitialized; }
        }



        public void InitializeOnEnterRuntime()
        {
            foreach (Variable variable in _clonedVariables)
            {
                Debug.Assert(variable.guid.IsEmpty() == false, "Variable GUID cannot be empty");
                
                Debug.Assert(variable.key.IsNotNullOrEmpty(), "Variable key cannot be null or empty");
                
                _runtimeVariables.Add(variable.key, variable);
            }

            _isInitialized = true;
        }


        public bool CanUpdateable(UGUID lastAppliedVersion)
        {
            if (this._appliedVersion != lastAppliedVersion)
            {
                this._appliedVersion = lastAppliedVersion;
                return true;
            }
            else
            {
                return false;
            }
        }


        public Variable FindVariable(string variableKey)
        {
            if (string.IsNullOrEmpty(variableKey))
            {
                return null;
            }
            
            if (isInitialized == false)
            {
                return _clonedVariables.Find(v => v.key == variableKey);
            }
            
            if (_runtimeVariables.TryGetValue(variableKey, out Variable variable))
            {
                return variable;
            }

            return null;
        }
        

        public Variable FindVariable(in UGUID key)
        {
            if (key.IsEmpty())
            {
                return null;
            }

            UGUID guid = key;

            return _clonedVariables.Find(v => v.guid == guid);
        }


        public void AddVariable(Variable variable)
        {
            _clonedVariables.Add(variable);
        }


        public void RemoveVariable(Variable variable)
        {
            _clonedVariables.Remove(variable);
        }


        public bool HasVariable(string key)
        {
            if (_clonedVariables.Find(v => v.key == key) is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public void ClearVariables()
        {
            _clonedVariables.Clear();
        }
    }
}