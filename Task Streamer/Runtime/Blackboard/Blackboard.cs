using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TaskStreamer.Utility;
using UnityEngine;
using UnityEngine.Pool;

namespace TaskStreamer
{
    public class Blackboard : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeReference, HideInInspector]
        private List<Variable> _variables = new List<Variable>();

        private readonly Dictionary<int, Variable> _variableCache = new Dictionary<int, Variable>();


#if UNITY_EDITOR
        internal List<Variable> variables
        {
            get { return _variables; }
        }
#endif


        public Variable FindVariable(in string key)
        {
            int hashCode = Utilities.StringToHash(key);

            if (hashCode == -1)
            {
                return null;
            }

            if (_variableCache.TryGetValue(hashCode, out Variable result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }


#if UNITY_EDITOR
        internal Variable[] GetVariablesByType(Type variableType)
        {
            List<Variable> variableList = ListPool<Variable>.Get();

            for (int index = 0; index < this.variables.Count; ++index)
            {
                Variable variable = this.variables[index];
                
                if (string.IsNullOrEmpty(variable.name))
                {
                    Debug.LogError($"Invalid name: [{index}]{variable.type.Name}");
                    continue;
                }

                if (variableType.IsAssignableFrom(variable.GetType()))
                {
                    variableList.Add(variable);
                }
            }

            Variable[] resultVariables = variableList.ToArray();
            ListPool<Variable>.Release(variableList);
            return resultVariables;
        }
        
        
        internal void AddVariable(Variable variable)
        {
            Variable foundVariable = this.FindVariable(variable.name);

            if (foundVariable != null)
            {
                variable.name = this.GenerateUniqueVariableName(variable.name);
            }

            _variables.Add(variable);
            _variableCache.Add(variable.nameHash, variable);
        }


        internal void RemoveVariable(Variable variable)
        {
            Variable foundVariable = this.FindVariable(variable.name);

            if (foundVariable == null)
            {
                Debug.LogError("해당 블랙보드 변수를 찾을 수 없습니다.");
            }
            else
            {
                _variables.Remove(variable);
                _variableCache.Remove(variable.nameHash);
            }
        }
#endif


        public bool TryChangeVariableName(Variable variable, in string newName)
        {
            Variable foundVariable = this.FindVariable(newName);
            
            //변경 사항이 없다면 그냥 반환한다.
            if (foundVariable == variable)
            {
                return true;
            }
            
            //재등록하기 위해 우선, 제거한다.
            _variableCache.Remove(variable.nameHash);

            //동일한 이름의 BBVariable이 있다면, 현재 이름 뒤에 인덱스를 붙이고 재등록.
            if (foundVariable != null)
            {
                variable.name = this.GenerateUniqueVariableName(newName);
                _variableCache.Add(variable.nameHash, variable);
                return true;
            }
            else //동일한 이름이 없다면 Hash를 Key로 재등록한다.
            {
                variable.name = newName;
                _variableCache.Add(variable.nameHash, variable);
                return true;
            }
        }


        private string GenerateUniqueVariableName(string variableName)
        {
            int newIndex = 0;
            string baseKey = variableName;
            Match match = Regex.Match(variableName, @"\((\d+)\)$");

            if (match.Success)
            {
                baseKey = variableName.Substring(0, match.Index);
                baseKey = baseKey.TrimEnd();
            }

            while (this.FindVariable($"{baseKey} ({newIndex})") != null)
            {
                newIndex++;
            }

            return $"{baseKey} ({newIndex})";
        }


        public void OnBeforeSerialize() { }


        public void OnAfterDeserialize()
        {
            if (_variables is null || _variableCache is null)
            {
                return;
            }

            _variableCache.Clear();

            foreach (Variable variable in _variables)
            {
                _variableCache.Add(variable.nameHash, variable);
            }
        }
    }
}