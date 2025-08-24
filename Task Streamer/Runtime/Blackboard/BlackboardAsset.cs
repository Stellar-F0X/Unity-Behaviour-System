using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Utility;
using UnityEngine;
using UnityEngine.Pool;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaskStreamer
{
    public sealed class BlackboardAsset : ScriptableObject, IBlackboard
    {
        [SerializeField, HideInInspector]
        private UGUID _appliedVersion;

        [SerializeReference, HideInInspector]
        private List<BlackboardVariable> _variables = new List<BlackboardVariable>();

        
        internal UGUID appliedVersion
        {
            get { return _appliedVersion; }
        }

        internal List<BlackboardVariable> variables
        {
            get { return _variables; }
            
            set { _variables = value; }
        }

        public int count
        {
            get { return variables is null ? 0 : variables.Count; }
        }



        public void UpdateAppliedVersion()
        {
            this._appliedVersion = UGUID.Create();
        }
        

        public BlackboardVariable FindVariable(in UGUID key)
        {
            if (key.IsEmpty())
            {
                return null;
            }

            UGUID guid = key;

            return _variables.Find(v => v.guid == guid);
        }


        public BlackboardVariable FindVariable(string variableName)
        {
            int hashCode = StringUtility.StringToHash(variableName);

            if (hashCode == -1)
            {
                return null;
            }
            else
            {
                return _variables.Find(v => v.keyHash == hashCode);
            }
        }


        public string[] VariableNames(UGUID excluded = default)
        {
            if (excluded.IsEmpty())
            {
                return _variables.Select(v => v.key).ToArray();
            }
            else
            {
                return _variables.Where(v => v.guid != excluded).Select(v => v.key).ToArray();
            }
        }


#if UNITY_EDITOR
        public void AddVariable(BlackboardVariable variable)
        {
            this._appliedVersion = UGUID.Create();

            BlackboardVariable foundVariable = this.FindVariable(variable.key);

            if (foundVariable != null)
            {
                variable.key = ObjectNames.GetUniqueName(this.VariableNames(variable.guid), variable.key);
            }

            _variables.Add(variable);
        }


        public void RemoveVariable(BlackboardVariable variable)
        {
            this._appliedVersion = UGUID.Create();

            BlackboardVariable foundVariable = this.FindVariable(variable.guid);

            if (foundVariable == null)
            {
                Debug.LogError("Failed to find the specified blackboard variable.");
            }
            else
            {
                _variables.Remove(variable);
            }
        }


        public bool HasVariable(string key)
        {
            if (this.FindVariable(key) is null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        internal BlackboardVariable[] GetVariablesByType(Type variableType)
        {
            List<BlackboardVariable> variableList = ListPool<BlackboardVariable>.Get();

            foreach (BlackboardVariable variable in this.variables)
            {
                if (string.IsNullOrEmpty(variable.key))
                {
                    Debug.LogError($"Invalid key: {variable.type.Name}");
                    continue;
                }

                if (variableType.IsAssignableFrom(variable.GetType()))
                {
                    variableList.Add(variable);
                }
            }

            BlackboardVariable[] resultVariables = variableList.ToArray();
            ListPool<BlackboardVariable>.Release(variableList);
            return resultVariables;
        }


        internal bool TryRenameKey(BlackboardVariable variable, in string newKey)
        {
            //이미 변경하려는 이름과 같은 이름이라면 Early Return 한다.
            if (string.Compare(variable.key, newKey) == 0)
            {
                return true;
            }

            BlackboardVariable foundVariable = this.FindVariable(newKey);
            this._appliedVersion = UGUID.Create();

            //동일한 이름의 BBVariable이 있다면, 현재 이름 뒤에 인덱스를 붙인다.
            if (foundVariable != null)
            {
                variable.key = ObjectNames.GetUniqueName(this.VariableNames(variable.guid), newKey);
                return true;
            }
            else //동일한 이름이 없다면 Hash를 Key로 재등록한다.
            {
                variable.key = newKey;
                return true;
            }
        }
#endif
    }
}