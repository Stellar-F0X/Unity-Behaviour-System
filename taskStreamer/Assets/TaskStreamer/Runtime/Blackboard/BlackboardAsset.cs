using System;
using System.Collections.Generic;
using System.Linq;
using TaskStreamer.Runtime.Utility;
using UnityEngine;
using UnityEngine.Pool;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TaskStreamer.Runtime
{
    /// <summary> 블랙보드 데이터를 저장하는 ScriptableObject </summary>
    public sealed class BlackboardAsset : ScriptableObject
    {
        [SerializeField]
        private BlackboardData _blackboardData = new BlackboardData();
        
        
        /// <summary>
        /// Runtime 블랙보드와의 동기화를 위한 버전 관리 용도.
        /// 블랙보드 변수가 변경 또는 수정될 때마다 버전이 갱신되며, 
        /// 연결된 TS의 Runtime 블랙보드는 버전이 다를 경우에만 동기화를 수행한다. 
        /// </summary>
        internal UGUID appliedVersion
        {
            get { return _blackboardData.appliedVersion; }
        }
        

        /// <summary> 블랙보드에 적용된 버전 정보를 나타냅니다. </summary>
        internal List<BlackboardVariable> variables
        {
            get { return _blackboardData.variables; }
        }

        
        /// <summary>Gets the count of variables in the blackboard.</summary>
        public int count
        {
            get { return variables is null ? 0 : variables.Count; }
        }
        


        internal void ChangeBlackboardData(BlackboardData newData)
        {
            Debug.Assert(newData is not null, "newData is null");
            this._blackboardData = newData;
        }


        /// <summary> Updates the applied version of the BlackboardAsset. </summary>
        public void UpdateAppliedVersion()
        {
            Debug.Assert(this._blackboardData is not null, "BlackboardData is null");
            this._blackboardData.UpdateAppliedVersion();
        }


        /// <summary> 지정된 GUID 키에 해당하는 BlackboardVariable을 반환합니다. </summary>
        /// <param name="key">찾고자 하는 변수의 고유 식별자(GUID).</param>
        /// <returns>해당 키에 매칭되는 BlackboardVariable 객체. 없으면 null 반환.</returns>
        public BlackboardVariable FindVariable(in UGUID key)
        {
            Debug.Assert(this._blackboardData is not null, "BlackboardData is null");
            return _blackboardData.FindVariable(key);
        }


        /// <summary> 주어진 변수 이름으로 블랙보드 변수 객체를 검색합니다. </summary>
        /// <param name="variableName">검색할 변수의 이름입니다.</param>
        /// <returns>검색된 블랙보드 변수 객체를 반환하며, 존재하지 않을 경우 null을 반환합니다.</returns>
        public BlackboardVariable FindVariable(string variableName)
        {
            Debug.Assert(this._blackboardData is not null, "BlackboardData is null");
            return _blackboardData.FindVariable(variableName);
        }


        /// <summary> 특정 GUID를 제외하거나 전체 변수 이름을 배열로 반환합니다. </summary>
        /// <param name="excluded"> 제외할 GUID 값 (기본값은 default) </param>
        /// <returns> 변수 이름 배열 </returns>
        private string[] VariableNames(UGUID excluded = default)
        {
            if (excluded.IsEmpty())
            {
                return variables.Select(v => v.key).ToArray();
            }
            else
            {
                return variables.Where(v => v.guid != excluded).Select(v => v.key).ToArray();
            }
        }
        
        
        /// <summary>지정된 키를 가진 변수가 존재하는지 확인합니다.</summary>
        /// <param name="key">확인할 변수의 GUID 키입니다.</param>
        /// <returns>변수가 존재하면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool HasVariable(UGUID key)
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


#if UNITY_EDITOR
        /// <summary> 새로운 변수를 블랙보드에 추가합니다. </summary>
        /// <param name="variable"> 추가할 BlackboardVariable 객체입니다. </param>
        public void AddVariable(BlackboardVariable variable)
        {
            this._blackboardData.UpdateAppliedVersion();

            BlackboardVariable foundVariable = this.FindVariable(variable.key);

            if (foundVariable != null)
            {
                variable.key = ObjectNames.GetUniqueName(this.VariableNames(variable.guid), variable.key);
            }

            _blackboardData.AddVariable(variable);
        }


        /// <summary>블랙보드에서 지정된 변수를 제거합니다.</summary>
        /// <param name="variable">제거하려는 블랙보드 변수입니다.</param>
        public void RemoveVariable(BlackboardVariable variable)
        {
            this._blackboardData.UpdateAppliedVersion();

            BlackboardVariable foundVariable = this.FindVariable(variable.guid);

            if (foundVariable is not null)
            {
                _blackboardData.RemoveVariable(variable);
            }
            else
            {
                Debug.LogError("Failed to find the specified blackboard variable.");
            }
        }


        /// <summary> 주어진 타입에 해당하는 BlackboardVariable들을 반환한다. </summary>
        /// <param name="variableType"> 검색하려는 변수들의 타입 </param>
        /// <returns> 주어진 타입과 호환되는 BlackboardVariable 배열 </returns>
        internal BlackboardVariable[] GetVariablesByType(Type variableType)
        {
            List<BlackboardVariable> variableList = ListPool<BlackboardVariable>.Get();

            foreach (BlackboardVariable variable in this.variables)
            {
                if (string.IsNullOrEmpty(variable.key))
                {
                    Debug.LogError($"Invalid key: {variable.genericVariableType.Name}");
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


        /// <summary> 지정된 변수의 키를 새로운 키로 변경 시도합니다. </summary>
        /// <param name="variable">키를 변경하려는 BlackboardVariable 객체입니다.</param>
        /// <param name="newKey">변경할 새로운 키 값입니다.</param>
        /// <returns>키 변경에 성공하면 true를, 실패하면 false를 반환합니다.</returns>
        internal bool TryRenameKey(BlackboardVariable variable, in string newKey)
        {
            // Null 검증
            if (variable == null || string.IsNullOrWhiteSpace(newKey))
            {
                return false;
            }
    
            // FindVariable과 동일한 해시 기반 비교 사용
            if (variable.keyHash == StringUtility.StringToHash(newKey))
            {
                return true;
            }

            BlackboardVariable foundVariable = this.FindVariable(newKey);
            
            this._blackboardData.UpdateAppliedVersion();

            //동일한 이름의 BBVariable이 있다면, 현재 이름 뒤에 인덱스를 붙인다.
            if (foundVariable != null)
            {
                variable.key = ObjectNames.GetUniqueName(this.VariableNames(variable.guid), newKey);
            }
            else //동일한 이름이 없다면 Hash를 Key로 재등록한다.
            {
                variable.key = newKey;
            }

            return true;
        }
#endif
    }
}